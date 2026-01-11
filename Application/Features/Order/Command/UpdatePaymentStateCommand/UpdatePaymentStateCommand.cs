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

namespace Application.Features.Order.Command.UpdatePaymentStateCommand
{
    public record UpdatePaymentStateCommand : IRequest<Result<OrderPaymentDto>>
    {
        public int OrderId { get; set; }
        public PaymentState NewState { get; set; }
    }

    public class UpdatePaymentStateCommandHandler : IRequestHandler<UpdatePaymentStateCommand, Result<OrderPaymentDto>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;

        public UpdatePaymentStateCommandHandler(DatabaseContext context, IUserSession userSession)
        {
            _context = context;
            _userSession = userSession;
        }

        public async Task<Result<OrderPaymentDto>> Handle(UpdatePaymentStateCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.OrderPayments)
                .Include(o => o.OrderCancellationFee)
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

            if (order == null)
            {
                return Result.Failure<OrderPaymentDto>($"Order with ID {request.OrderId} not found");
            }

            var orderPayment = order.OrderPayments.FirstOrDefault();
            if (orderPayment == null)
            {
                return Result.Failure<OrderPaymentDto>("Order payment not found");
            }

            try
            {
                // Update payment state
                switch (request.NewState)
                {
                    case PaymentState.Paid:
                        orderPayment.MarkAsPaid(_userSession.UserName ?? "System");
                        break;
                    case PaymentState.Failed:
                        orderPayment.MarkAsFailed(_userSession.UserName ?? "System");
                        break;
                    default:
                        return Result.Failure<OrderPaymentDto>($"Cannot update payment state to {request.NewState}");
                }

                // If payment is marked as Paid and cancellation fee exists, mark cancellation fee as Paid
                if (request.NewState == PaymentState.Paid && order.OrderCancellationFee != null && order.OrderCancellationFee.State == CancellationFeeState.NotYet)
                {
                    order.OrderCancellationFee.MarkAsPaid(_userSession.UserName ?? "System");

                    // Update treasury with cancellation fee
                    var treasury = await _context.CompanyTreasuries.FirstOrDefaultAsync(cancellationToken);
                    if (treasury == null)
                    {
                        treasury = CompanyTreasury.Create(_userSession.UserName ?? "System");
                        _context.CompanyTreasuries.Add(treasury);
                        await _context.SaveChangesAsync(cancellationToken);
                    }

                    TreasuryService.AddCancellationFee(treasury, order.OrderCancellationFee.Amount, _userSession.UserName ?? "System");
                }

                await _context.SaveChangesAsync(cancellationToken);

                var paymentDto = new OrderPaymentDto
                {
                    Id = orderPayment.Id,
                    OrderId = orderPayment.OrderId,
                    PaymentMethod = (PaymentMethod)orderPayment.PaymentMethodId,
                    Total = orderPayment.Total,
                    State = orderPayment.State,
                    CreatedDate = orderPayment.CreatedDate
                };

                return Result.Success(paymentDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<OrderPaymentDto>($"Error updating payment state: {ex.Message}");
            }
        }
    }
}

