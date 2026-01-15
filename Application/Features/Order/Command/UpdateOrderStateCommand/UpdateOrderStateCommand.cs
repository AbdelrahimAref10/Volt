using Application.Features.Order.DTOs;
using CSharpFunctionalExtensions;
using Domain.Common;
using Domain.Enums;
using Domain.Models;
using Domain.Services;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Command.UpdateOrderStateCommand
{
    public record UpdateOrderStateCommand : IRequest<Result<OrderDto>>
    {
        public int OrderId { get; set; }
        public OrderState NewState { get; set; }
        public List<int>? VehicleIds { get; set; } // For confirmation only
    }

    public class UpdateOrderStateCommandHandler : IRequestHandler<UpdateOrderStateCommand, Result<OrderDto>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;

        public UpdateOrderStateCommandHandler(DatabaseContext context, IUserSession userSession)
        {
            _context = context;
            _userSession = userSession;
        }

        public async Task<Result<OrderDto>> Handle(UpdateOrderStateCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.SubCategory)
                .Include(o => o.City)
                .Include(o => o.OrderVehicles)
                    .ThenInclude(ov => ov.Vehicle)
                .Include(o => o.OrderPayments)
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

            if (order == null)
            {
                return Result.Failure<OrderDto>($"Order with ID {request.OrderId} not found");
            }

            try
            {
                // Update state based on transition
                switch (request.NewState)
                {
                    case OrderState.Confirmed:
                        if (order.OrderState != OrderState.Pending)
                        {
                            return Result.Failure<OrderDto>($"Cannot confirm order. Current state: {order.OrderState}");
                        }

                        // Validate vehicle IDs are provided
                        if (request.VehicleIds == null || request.VehicleIds.Count == 0)
                        {
                            return Result.Failure<OrderDto>("Vehicle IDs are required to confirm order");
                        }

                        if (request.VehicleIds.Count != order.VehiclesCount)
                        {
                            return Result.Failure<OrderDto>($"Number of vehicles ({request.VehicleIds.Count}) must match order vehicles count ({order.VehiclesCount})");
                        }

                        // Get and validate vehicles
                        var vehicles = await _context.Vehicles
                            .Where(v => request.VehicleIds.Contains(v.VehicleId))
                            .ToListAsync(cancellationToken);

                        if (vehicles.Count != request.VehicleIds.Count)
                        {
                            return Result.Failure<OrderDto>("One or more vehicles not found");
                        }

                        // Validate all vehicles
                        foreach (var vehicle in vehicles)
                        {
                            // Check vehicle has VehicleCode
                            if (string.IsNullOrWhiteSpace(vehicle.VehicleCode))
                            {
                                return Result.Failure<OrderDto>($"Vehicle ID {vehicle.VehicleId} does not have a vehicle code assigned");
                            }

                            // Check vehicle is Available
                            if (vehicle.Status != "Available")
                            {
                                return Result.Failure<OrderDto>($"Vehicle {vehicle.VehicleCode} is not available. Current status: {vehicle.Status}");
                            }

                            // Check vehicle belongs to order's SubCategory
                            if (vehicle.SubCategoryId != order.SubCategoryId)
                            {
                                return Result.Failure<OrderDto>($"Vehicle {vehicle.VehicleCode} does not belong to order's subcategory");
                            }

                            // Check vehicle is not reserved in date range
                            var isReserved = await _context.ReservedVehiclesPerDays
                                .Include(rv => rv.Order)
                                .AnyAsync(rv => rv.VehicleId == vehicle.VehicleId
                                    && rv.State == ReservedVehicleState.StillBooked
                                    && rv.Order.OrderState != OrderState.Completed
                                    && ((rv.DateFrom <= order.ReservationDateTo && rv.DateTo >= order.ReservationDateFrom)), cancellationToken);

                            if (isReserved)
                            {
                                return Result.Failure<OrderDto>($"Vehicle {vehicle.VehicleCode} is already reserved in the selected date range");
                            }
                        }

                        // Confirm order
                        order.Confirm(_userSession.UserName ?? "System");

                        // Create OrderVehicle records
                        foreach (var vehicleId in request.VehicleIds)
                        {
                            var orderVehicle = Domain.Models.OrderVehicle.Create(
                                order.OrderId,
                                vehicleId,
                                _userSession.UserName ?? "System"
                            );
                            _context.OrderVehicles.Add(orderVehicle);
                        }

                        // Create ReservedVehiclesPerDays records (one per vehicle per day)
                        foreach (var vehicle in vehicles)
                        {
                            var currentDate = order.ReservationDateFrom.Date;
                            var endDate = order.ReservationDateTo.Date;

                            while (currentDate <= endDate)
                            {
                                var reserved = Domain.Models.ReservedVehiclesPerDays.Create(
                                    vehicle.VehicleId,
                                    order.SubCategoryId,
                                    vehicle.VehicleCode,
                                    order.OrderId,
                                    currentDate,
                                    currentDate,
                                    _userSession.UserName ?? "System"
                                );
                                _context.ReservedVehiclesPerDays.Add(reserved);
                                currentDate = currentDate.AddDays(1);
                            }

                            // Update vehicle status to Rented
                            vehicle.UpdateStatus("Rented", _userSession.UserName ?? "System");
                        }

                        // Note: PayPal payment treasury record creation is removed - will be handled automatically when payment is successful
                        break;

                    case OrderState.OnWay:
                        if (order.OrderState != OrderState.Confirmed)
                        {
                            return Result.Failure<OrderDto>($"Cannot mark order as OnWay. Current state: {order.OrderState}");
                        }
                        order.MarkOnWay(_userSession.UserName ?? "System");
                        break;

                    case OrderState.CustomerReceived:
                        if (order.OrderState != OrderState.OnWay)
                        {
                            return Result.Failure<OrderDto>($"Cannot mark customer received. Current state: {order.OrderState}");
                        }
                        order.MarkCustomerReceived(_userSession.UserName ?? "System");
                        
                        // If payment method is Cash, automatically mark payment as Paid and create treasury record
                        if (order.PaymentMethodId == (int)PaymentMethod.Cash)
                        {
                            var orderPayment = order.OrderPayments.FirstOrDefault();
                            if (orderPayment != null && orderPayment.State == PaymentState.Pending)
                            {
                                orderPayment.MarkAsPaid(_userSession.UserName ?? "System");
                                
                                // Create treasury record for cash payment
                                var treasuryRecord = TreasuryService.CreateCashPaymentRecord(
                                    orderPayment.Total,
                                    order.OrderCode,
                                    _userSession.UserName ?? "System");
                                _context.CompanyTreasuries.Add(treasuryRecord);
                            }
                        }
                        break;

                    case OrderState.Completed:
                        if (order.OrderState != OrderState.CustomerReceived)
                        {
                            return Result.Failure<OrderDto>($"Cannot complete order. Current state: {order.OrderState}");
                        }

                        // Update vehicle status to Available
                        foreach (var orderVehicle in order.OrderVehicles)
                        {
                            orderVehicle.Vehicle.UpdateStatus("Available", _userSession.UserName ?? "System");
                        }

                        order.Complete(_userSession.UserName ?? "System");
                        break;

                    default:
                        return Result.Failure<OrderDto>($"Invalid state transition to {request.NewState}");
                }

                await _context.SaveChangesAsync(cancellationToken);

                // Return updated order DTO
                var orderDto = new OrderDto
                {
                    OrderId = order.OrderId,
                    OrderCode = order.OrderCode,
                    CustomerId = order.CustomerId,
                    CustomerName = order.Customer.FullName,
                    SubCategoryId = order.SubCategoryId,
                    SubCategoryName = order.SubCategory.Name,
                    CityId = order.CityId,
                    CityName = order.City.Name,
                    ReservationDateFrom = order.ReservationDateFrom,
                    ReservationDateTo = order.ReservationDateTo,
                    VehiclesCount = order.VehiclesCount,
                    OrderSubTotal = order.OrderSubTotal,
                    OrderTotal = order.OrderTotal,
                    Notes = order.Notes,
                    HotelName = order.HotelName,
                    HotelAddress = order.HotelAddress,
                    HotelPhone = order.HotelPhone,
                    IsUrgent = order.IsUrgent,
                    PaymentMethod = (PaymentMethod)order.PaymentMethodId,
                    OrderState = order.OrderState,
                    CreatedDate = order.CreatedDate
                };

                return Result.Success(orderDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<OrderDto>($"Error updating order state: {ex.Message}");
            }
        }
    }
}

