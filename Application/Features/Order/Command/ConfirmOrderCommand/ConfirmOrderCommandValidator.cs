using CSharpFunctionalExtensions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Command.ConfirmOrderCommand
{
    public class ConfirmOrderCommandValidator
    {
        private readonly DatabaseContext _context;

        public ConfirmOrderCommandValidator(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result> ValidateAsync(ConfirmOrderCommand request, CancellationToken cancellationToken)
        {
            if (request.OrderId <= 0)
            {
                return Result.Failure("Order ID is required");
            }

            if (request.VehicleIds == null || !request.VehicleIds.Any())
            {
                return Result.Failure("At least one vehicle must be assigned");
            }

            var orderExists = await _context.Orders
                .AnyAsync(o => o.OrderId == request.OrderId, cancellationToken);

            if (!orderExists)
            {
                return Result.Failure("Order not found");
            }

            // Check for duplicate vehicle IDs
            if (request.VehicleIds.Count != request.VehicleIds.Distinct().Count())
            {
                return Result.Failure("Duplicate vehicle IDs are not allowed");
            }

            return Result.Success();
        }
    }
}

