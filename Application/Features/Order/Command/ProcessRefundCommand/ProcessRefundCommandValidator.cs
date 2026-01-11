using CSharpFunctionalExtensions;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Command.ProcessRefundCommand
{
    public class ProcessRefundCommandValidator
    {
        private readonly DatabaseContext _context;

        public ProcessRefundCommandValidator(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result> ValidateAsync(ProcessRefundCommand request, CancellationToken cancellationToken)
        {
            if (request.OrderId <= 0)
            {
                return Result.Failure("Order ID is required");
            }

            if (!Enum.IsDefined(typeof(RefundState), request.RefundState))
            {
                return Result.Failure("Invalid refund state");
            }

            // Only allow Success or Failed states
            if (request.RefundState != RefundState.Success && request.RefundState != RefundState.Failed)
            {
                return Result.Failure("Only Success or Failed states can be set");
            }

            var refundableExists = await _context.RefundablePaypalAmounts
                .AnyAsync(rpa => rpa.OrderId == request.OrderId, cancellationToken);

            if (!refundableExists)
            {
                return Result.Failure("Refundable PayPal amount not found for this order");
            }

            return Result.Success();
        }
    }
}

