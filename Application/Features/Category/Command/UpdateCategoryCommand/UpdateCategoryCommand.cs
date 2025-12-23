using Application.Features.Category.DTOs;
using CSharpFunctionalExtensions;
using Domain.Common;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Category.Command.UpdateCategoryCommand
{
    public record UpdateCategoryCommand : IRequest<Result<CategoryDto>>
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;
        private readonly UpdateCategoryCommandValidator _validator;

        public UpdateCategoryCommandHandler(
            DatabaseContext context, 
            IUserSession userSession,
            UpdateCategoryCommandValidator validator)
        {
            _context = context;
            _userSession = userSession;
            _validator = validator;
        }

        public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            // Validate command using validator
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<CategoryDto>(validationResult.Error);
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId, cancellationToken);

            try
            {
                category.Update(
                    request.Name,
                    request.Description,
                    request.ImageUrl,
                    _userSession.UserName ?? "System"
                );

                await _context.SaveChangesAsync(cancellationToken);

                var vehicleCount = await _context.Vehicles
                    .CountAsync(v => v.CategoryId == category.CategoryId, cancellationToken);

                var categoryDto = new CategoryDto
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    IsActive = category.IsActive,
                    VehicleCount = vehicleCount
                };

                return Result.Success(categoryDto);
            }
            catch (System.Exception ex)
            {
                return Result.Failure<CategoryDto>($"Error updating category: {ex.Message}");
            }
        }
    }
}


