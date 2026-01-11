using CSharpFunctionalExtensions;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Command.UpdatePaymentStateCommand
{
    public class UpdatePaymentStateCommandValidator
    {
        private readonly DatabaseContext _context;

        public UpdatePaymentStateCommandValidator(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result> ValidateAsync(UpdatePaymentStateCommand request, CancellationToken cancellationToken)
        {
            if (request.OrderId <= 0)
            {
                return Result.Failure("Order ID is required");
            }

            if (!Enum.IsDefined(typeof(PaymentState), request.NewState))
            {
                return Result.Failure("Invalid payment state");
            }

            // Only allow Paid or Failed states for manual updates
            if (request.NewState != PaymentState.Paid && request.NewState != PaymentState.Failed)
            {
                return Result.Failure("Only Paid or Failed states can be set manually");
            }

            var orderExists = await _context.Orders
                .AnyAsync(o => o.OrderId == request.OrderId, cancellationToken);

            if (!orderExists)
            {
                return Result.Failure("Order not found");
            }

            return Result.Success();
        }
    }
}

