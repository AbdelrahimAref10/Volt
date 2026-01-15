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

namespace Application.Features.Order.Command.CancelOrderCommand
{
    public record CancelOrderCommand : IRequest<Result<bool>>
    {
        public int OrderId { get; set; }
    }

    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<bool>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;

        public CancelOrderCommandHandler(DatabaseContext context, IUserSession userSession)
        {
            _context = context;
            _userSession = userSession;
        }

        public async Task<Result<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.City)
                .Include(o => o.OrderPayments)
                .Include(o => o.OrderVehicles)
                    .ThenInclude(ov => ov.Vehicle)
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

            if (order == null)
            {
                return Result.Failure<bool>($"Order with ID {request.OrderId} not found");
            }

            // Validate order can be cancelled
            if (order.OrderState == OrderState.Completed)
            {
                return Result.Failure<bool>("Cannot cancel a completed order");
            }

            // Check if user has permission (customer can only cancel their own orders, admin can cancel any)
            var userId = _userSession.UserId;
            var isAdmin = _userSession.Roles.Contains("Admin");

            if (!isAdmin && order.CustomerId != userId)
            {
                return Result.Failure<bool>("You do not have permission to cancel this order");
            }

            try
            {
                // Calculate cancellation fee (4 days policy)
                var cancellationFee = OrderCalculationService.CalculateCancellationFee(order.City, order.CreatedDate);

                // Create cancellation fee if applicable
                if (cancellationFee.HasValue && cancellationFee.Value > 0)
                {
                    var orderCancellationFee = Domain.Models.OrderCancellationFee.Create(
                        order.CustomerId,
                        order.OrderId,
                        cancellationFee.Value,
                        _userSession.UserName ?? "System"
                    );

                    _context.OrderCancellationFees.Add(orderCancellationFee);
                }

                // Handle refund if PayPal payment
                var orderPayment = order.OrderPayments.FirstOrDefault();
                if (orderPayment != null && orderPayment.PaymentMethodId == (int)PaymentMethod.PayPal)
                {
                    var refundableAmount = order.OrderTotal - (cancellationFee ?? 0);

                    if (refundableAmount > 0)
                    {
                        var refundablePaypal = Domain.Models.RefundablePaypalAmount.Create(
                            order.CustomerId,
                            order.OrderId,
                            order.OrderTotal,
                            cancellationFee ?? 0,
                            _userSession.UserName ?? "System"
                        );

                        _context.RefundablePaypalAmounts.Add(refundablePaypal);

                        // TODO: Process PayPal refund
                        // For now, mark as Pending
                    }

                    // Mark payment as refunded
                    orderPayment.MarkAsRefunded(_userSession.UserName ?? "System");
                }

                // Update vehicle status if vehicles were assigned
                if (order.OrderState == OrderState.Confirmed)
                {
                    foreach (var orderVehicle in order.OrderVehicles)
                    {
                        orderVehicle.Vehicle.UpdateStatus("Available", _userSession.UserName ?? "System");
                    }
                }

                // Update ReservedVehiclesPerDays state to Cancelled
                var reservedVehicles = await _context.ReservedVehiclesPerDays
                    .Where(rv => rv.OrderId == order.OrderId)
                    .ToListAsync(cancellationToken);

                foreach (var reserved in reservedVehicles)
                {
                    reserved.Cancel(_userSession.UserName ?? "System");
                }

                // Cancel the order
                order.Cancel(_userSession.UserName ?? "System");

                // TODO: Add treasury record handling for cancellation
                // This will be implemented soon - treasury records will be created when cancellation fees are paid

                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success(true);
            }
            catch (Exception ex)
            {
                return Result.Failure<bool>($"Error cancelling order: {ex.Message}");
            }
        }
    }
}

