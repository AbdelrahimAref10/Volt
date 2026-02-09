using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.City.Command.AddCityCommand
{
    public class AddCityCommandValidator
    {
        private readonly DatabaseContext _context;

        public AddCityCommandValidator(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result> ValidateAsync(AddCityCommand request, CancellationToken cancellationToken)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Result.Failure("City name is required");
            }

            if (request.Name.Trim().Length < 2)
            {
                return Result.Failure("City name must be at least 2 characters long");
            }

            // Validate fees are non-negative when provided
            if (request.DeliveryFees.HasValue && request.DeliveryFees.Value < 0)
                return Result.Failure("Delivery fees cannot be negative");
            if (request.UrgentDelivery.HasValue && request.UrgentDelivery.Value < 0)
                return Result.Failure("Urgent delivery fees cannot be negative");
            if (request.ServiceFees.HasValue && request.ServiceFees.Value < 0)
                return Result.Failure("Service fees cannot be negative");
            if (request.CancellationFees.HasValue && request.CancellationFees.Value < 0)
                return Result.Failure("Cancellation fees cannot be negative");

            // Check if city with same name already exists
            var existingCity = await _context.Cities
                .FirstOrDefaultAsync(c => c.Name.ToLower() == request.Name.ToLower().Trim(), cancellationToken);

            if (existingCity != null)
            {
                return Result.Failure("A city with this name already exists");
            }

            return Result.Success();
        }
    }
}

