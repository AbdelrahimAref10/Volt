using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.City.Command.UpdateCityCommand
{
    public class UpdateCityCommandValidator
    {
        private readonly DatabaseContext _context;

        public UpdateCityCommandValidator(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result> ValidateAsync(UpdateCityCommand request, CancellationToken cancellationToken)
        {
            if (request.CityId <= 0)
            {
                return Result.Failure("Valid city ID is required");
            }

            // Validate that city exists
            var cityExists = await _context.Cities.AnyAsync(c => c.CityId == request.CityId, cancellationToken);
            if (!cityExists)
            {
                return Result.Failure($"City with ID {request.CityId} not found");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Result.Failure("City name is required");
            }

            if (request.Name.Trim().Length < 2)
            {
                return Result.Failure("City name must be at least 2 characters long");
            }

            // Check if another city with same name exists
            var existingCity = await _context.Cities
                .FirstOrDefaultAsync(c => c.Name.ToLower() == request.Name.ToLower().Trim() && c.CityId != request.CityId, cancellationToken);

            if (existingCity != null)
            {
                return Result.Failure("A city with this name already exists");
            }

            return Result.Success();
        }
    }
}

