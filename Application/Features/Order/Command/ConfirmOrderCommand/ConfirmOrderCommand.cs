using Application.Features.Order.DTOs;
using CSharpFunctionalExtensions;
using Domain.Common;
using Domain.Enums;
using Domain.Models;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Command.ConfirmOrderCommand
{
    public record ConfirmOrderCommand : IRequest<Result<OrderDetailDto>>
    {
        public int OrderId { get; set; }
        public List<int> VehicleIds { get; set; } = new List<int>();
    }

    public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, Result<OrderDetailDto>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;

        public ConfirmOrderCommandHandler(DatabaseContext context, IUserSession userSession)
        {
            _context = context;
            _userSession = userSession;
        }

        public async Task<Result<OrderDetailDto>> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
        {
            // Validate command
            var validator = new ConfirmOrderCommandValidator(_context);
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<OrderDetailDto>(validationResult.Error);
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.SubCategory)
                .Include(o => o.City)
                .Include(o => o.OrderPayments)
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

            if (order == null)
            {
                return Result.Failure<OrderDetailDto>($"Order with ID {request.OrderId} not found");
            }

            if (order.OrderState != OrderState.Pending)
            {
                return Result.Failure<OrderDetailDto>($"Order must be in Pending state to confirm. Current state: {order.OrderState}");
            }

            if (request.VehicleIds.Count != order.VehiclesCount)
            {
                return Result.Failure<OrderDetailDto>($"Number of vehicles ({request.VehicleIds.Count}) must match order vehicles count ({order.VehiclesCount})");
            }

            // Get and validate vehicles
            var vehicles = await _context.Vehicles
                .Where(v => request.VehicleIds.Contains(v.VehicleId))
                .ToListAsync(cancellationToken);

            if (vehicles.Count != request.VehicleIds.Count)
            {
                return Result.Failure<OrderDetailDto>("One or more vehicles not found");
            }

            // Validate all vehicles
            foreach (var vehicle in vehicles)
            {
                // Check vehicle has VehicleCode
                if (string.IsNullOrWhiteSpace(vehicle.VehicleCode))
                {
                    return Result.Failure<OrderDetailDto>($"Vehicle ID {vehicle.VehicleId} does not have a vehicle code assigned");
                }

                // Check vehicle is Available
                if (vehicle.Status != "Available")
                {
                    return Result.Failure<OrderDetailDto>($"Vehicle {vehicle.VehicleCode} is not available. Current status: {vehicle.Status}");
                }

                // Check vehicle belongs to order's SubCategory
                if (vehicle.SubCategoryId != order.SubCategoryId)
                {
                    return Result.Failure<OrderDetailDto>($"Vehicle {vehicle.VehicleCode} does not belong to order's subcategory");
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
                    return Result.Failure<OrderDetailDto>($"Vehicle {vehicle.VehicleCode} is already reserved in the selected date range");
                }
            }

            try
            {
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
                    // Ensure VehicleCode is not null or empty
                    if (string.IsNullOrWhiteSpace(vehicle.VehicleCode))
                    {
                        return Result.Failure<OrderDetailDto>($"Vehicle ID {vehicle.VehicleId} does not have a vehicle code assigned. Please assign a vehicle code before confirming the order.");
                    }

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

                // Update payment state if PayPal (mark as Paid on confirmation)
                var orderPayment = order.OrderPayments.FirstOrDefault();
                if (orderPayment != null && orderPayment.PaymentMethodId == (int)PaymentMethod.PayPal && orderPayment.State == PaymentState.Pending)
                {
                    orderPayment.MarkAsPaid(_userSession.UserName ?? "System");
                }

                await _context.SaveChangesAsync(cancellationToken);

                // Return order detail
                var orderDetail = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.SubCategory)
                    .Include(o => o.City)
                    .Include(o => o.OrderVehicles)
                        .ThenInclude(ov => ov.Vehicle)
                    .Include(o => o.OrderPayments)
                    .Include(o => o.OrderCancellationFee)
                    .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

                if (orderDetail == null)
                {
                    return Result.Failure<OrderDetailDto>("Error retrieving order details");
                }

                var refundablePaypal = await _context.RefundablePaypalAmounts
                    .FirstOrDefaultAsync(rpa => rpa.OrderId == request.OrderId, cancellationToken);

                var orderDetailDto = new OrderDetailDto
                {
                    OrderId = orderDetail.OrderId,
                    OrderCode = orderDetail.OrderCode,
                    CustomerId = orderDetail.CustomerId,
                    CustomerName = orderDetail.Customer.FullName,
                    CustomerMobileNumber = orderDetail.Customer.MobileNumber,
                    SubCategoryId = orderDetail.SubCategoryId,
                    SubCategoryName = orderDetail.SubCategory.Name,
                    SubCategoryPrice = orderDetail.SubCategory.Price,
                    CityId = orderDetail.CityId,
                    CityName = orderDetail.City.Name,
                    ReservationDateFrom = orderDetail.ReservationDateFrom,
                    ReservationDateTo = orderDetail.ReservationDateTo,
                    VehiclesCount = orderDetail.VehiclesCount,
                    OrderSubTotal = orderDetail.OrderSubTotal,
                    OrderTotal = orderDetail.OrderTotal,
                    Notes = orderDetail.Notes,
                    PassportImage = orderDetail.PassportImage,
                    HotelName = orderDetail.HotelName,
                    HotelAddress = orderDetail.HotelAddress,
                    HotelPhone = orderDetail.HotelPhone,
                    IsUrgent = orderDetail.IsUrgent,
                    PaymentMethod = (PaymentMethod)orderDetail.PaymentMethodId,
                    OrderState = orderDetail.OrderState,
                    CreatedDate = orderDetail.CreatedDate,
                    OrderVehicles = orderDetail.OrderVehicles.Select(ov => new OrderVehicleDto
                    {
                        VehicleId = ov.VehicleId,
                        VehicleName = ov.Vehicle.Name,
                        VehicleCode = ov.Vehicle.VehicleCode,
                        Status = ov.Vehicle.Status
                    }).ToList(),
                    OrderPayments = orderDetail.OrderPayments.Select(op => new OrderPaymentDto
                    {
                        Id = op.Id,
                        OrderId = op.OrderId,
                        PaymentMethod = (PaymentMethod)op.PaymentMethodId,
                        Total = op.Total,
                        State = op.State,
                        CreatedDate = op.CreatedDate
                    }).ToList(),
                    OrderCancellationFee = orderDetail.OrderCancellationFee != null ? new OrderCancellationFeeDto
                    {
                        Id = orderDetail.OrderCancellationFee.Id,
                        CustomerId = orderDetail.OrderCancellationFee.CustomerId,
                        OrderId = orderDetail.OrderCancellationFee.OrderId,
                        Amount = orderDetail.OrderCancellationFee.Amount,
                        State = orderDetail.OrderCancellationFee.State,
                        CreatedDate = orderDetail.OrderCancellationFee.CreatedDate
                    } : null,
                    RefundablePaypalAmount = refundablePaypal != null ? new RefundablePaypalAmountDto
                    {
                        Id = refundablePaypal.Id,
                        CustomerId = refundablePaypal.CustomerId,
                        OrderId = refundablePaypal.OrderId,
                        OrderTotal = refundablePaypal.OrderTotal,
                        CancellationFees = refundablePaypal.CancellationFees,
                        RefundableAmount = refundablePaypal.RefundableAmount,
                        State = refundablePaypal.State,
                        CreatedDate = refundablePaypal.CreatedDate
                    } : null
                };

                return Result.Success(orderDetailDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<OrderDetailDto>($"Error confirming order: {ex.Message}");
            }
        }
    }
}

