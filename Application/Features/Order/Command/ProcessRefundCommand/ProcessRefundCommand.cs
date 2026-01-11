using Application.Features.Order.DTOs;
using CSharpFunctionalExtensions;
using Domain.Common;
using Domain.Enums;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Command.ProcessRefundCommand
{
    public record ProcessRefundCommand : IRequest<Result<RefundablePaypalAmountDto>>
    {
        public int OrderId { get; set; }
        public RefundState RefundState { get; set; }
    }

    public class ProcessRefundCommandHandler : IRequestHandler<ProcessRefundCommand, Result<RefundablePaypalAmountDto>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;

        public ProcessRefundCommandHandler(DatabaseContext context, IUserSession userSession)
        {
            _context = context;
            _userSession = userSession;
        }

        public async Task<Result<RefundablePaypalAmountDto>> Handle(ProcessRefundCommand request, CancellationToken cancellationToken)
        {
            var refundablePaypal = await _context.RefundablePaypalAmounts
                .Include(rpa => rpa.Order)
                    .ThenInclude(o => o.OrderPayments)
                .FirstOrDefaultAsync(rpa => rpa.OrderId == request.OrderId, cancellationToken);

            if (refundablePaypal == null)
            {
                return Result.Failure<RefundablePaypalAmountDto>($"Refundable PayPal amount for order {request.OrderId} not found");
            }

            if (refundablePaypal.State != RefundState.Pending)
            {
                return Result.Failure<RefundablePaypalAmountDto>($"Refund is not in Pending state. Current state: {refundablePaypal.State}");
            }

            try
            {
                // Update refund state
                if (request.RefundState == RefundState.Success)
                {
                    refundablePaypal.MarkAsSuccess(_userSession.UserName ?? "System");

                    // Update order payment state to Refunded
                    var orderPayment = refundablePaypal.Order.OrderPayments.FirstOrDefault();
                    if (orderPayment != null)
                    {
                        orderPayment.MarkAsRefunded(_userSession.UserName ?? "System");
                    }

                    // If cancellation fee exists, mark it as Paid
                    var cancellationFee = await _context.OrderCancellationFees
                        .FirstOrDefaultAsync(ocf => ocf.OrderId == request.OrderId, cancellationToken);

                    if (cancellationFee != null && cancellationFee.State == CancellationFeeState.NotYet)
                    {
                        cancellationFee.MarkAsPaid(_userSession.UserName ?? "System");

                        // Update treasury with cancellation fee
                        var treasury = await _context.CompanyTreasuries.FirstOrDefaultAsync(cancellationToken);
                        if (treasury == null)
                        {
                            treasury = Domain.Models.CompanyTreasury.Create(_userSession.UserName ?? "System");
                            _context.CompanyTreasuries.Add(treasury);
                            await _context.SaveChangesAsync(cancellationToken);
                        }

                        Domain.Services.TreasuryService.AddCancellationFee(treasury, cancellationFee.Amount, _userSession.UserName ?? "System");
                    }
                }
                else if (request.RefundState == RefundState.Failed)
                {
                    refundablePaypal.MarkAsFailed(_userSession.UserName ?? "System");
                }
                else
                {
                    return Result.Failure<RefundablePaypalAmountDto>($"Invalid refund state: {request.RefundState}");
                }

                await _context.SaveChangesAsync(cancellationToken);

                var refundDto = new RefundablePaypalAmountDto
                {
                    Id = refundablePaypal.Id,
                    CustomerId = refundablePaypal.CustomerId,
                    OrderId = refundablePaypal.OrderId,
                    OrderTotal = refundablePaypal.OrderTotal,
                    CancellationFees = refundablePaypal.CancellationFees,
                    RefundableAmount = refundablePaypal.RefundableAmount,
                    State = refundablePaypal.State,
                    CreatedDate = refundablePaypal.CreatedDate
                };

                return Result.Success(refundDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<RefundablePaypalAmountDto>($"Error processing refund: {ex.Message}");
            }
        }
    }
}

