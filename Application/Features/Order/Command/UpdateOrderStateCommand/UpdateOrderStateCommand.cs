using Application.Features.Order.DTOs;
using CSharpFunctionalExtensions;
using Domain.Common;
using Domain.Enums;
using Domain.Models;
using Domain.Services;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Command.UpdateOrderStateCommand
{
    public record UpdateOrderStateCommand : IRequest<Result<OrderDto>>
    {
        public int OrderId { get; set; }
        public OrderState NewState { get; set; }
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
            OrderPayment orderPayment; 
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
                        
                        // If payment method is Cash, automatically mark payment as Paid
                        if (order.PaymentMethodId == (int)PaymentMethod.Cash)
                        {
                            orderPayment = order.OrderPayments.FirstOrDefault();
                            if (orderPayment != null && orderPayment.State == PaymentState.Pending)
                            {
                                orderPayment.MarkAsPaid(_userSession.UserName ?? "System");
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

                        // Update treasury with order payment
                        orderPayment = order.OrderPayments.FirstOrDefault(op => op.State == PaymentState.Paid);
                        if (orderPayment != null)
                        {
                            var treasury = await _context.CompanyTreasuries.FirstOrDefaultAsync(cancellationToken);
                            if (treasury == null)
                            {
                                // Create treasury if it doesn't exist
                                treasury = CompanyTreasury.Create(_userSession.UserName ?? "System");
                                _context.CompanyTreasuries.Add(treasury);
                                await _context.SaveChangesAsync(cancellationToken);
                            }

                            TreasuryService.AddOrderRevenue(treasury, orderPayment.Total, _userSession.UserName ?? "System");
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

