using CSharpFunctionalExtensions;
using Domain.Common;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.City.Command.UpdateCityCommand
{
    public record UpdateCityCommand : IRequest<Result<int>>
    {
        public int CityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateCityCommandHandler : IRequestHandler<UpdateCityCommand, Result<int>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;
        private readonly UpdateCityCommandValidator _validator;

        public UpdateCityCommandHandler(
            DatabaseContext context, 
            IUserSession userSession,
            UpdateCityCommandValidator validator)
        {
            _context = context;
            _userSession = userSession;
            _validator = validator;
        }

        public async Task<Result<int>> Handle(UpdateCityCommand request, CancellationToken cancellationToken)
        {
            // Validate command using validator
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<int>(validationResult.Error);
            }

            var city = await _context.Cities
                .FirstOrDefaultAsync(c => c.CityId == request.CityId, cancellationToken);

            city.Update(
                request.Name,
                request.Description,
                _userSession.UserName ?? "System"
            );

            var saveResult = await _context.SaveChangesAsyncWithResult(cancellationToken);
            if (!saveResult.IsSuccess)
            {
                return Result.Failure<int>($"Failed to update city: {saveResult.ErrorMessage}");
            }

            return Result.Success(city.CityId);
        }
    }
}

