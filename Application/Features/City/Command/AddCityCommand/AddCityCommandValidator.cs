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

