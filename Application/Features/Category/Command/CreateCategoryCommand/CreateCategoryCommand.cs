using Application.Features.Category.DTOs;
using CSharpFunctionalExtensions;
using Domain.Common;
using Domain.Models;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Category.Command.CreateCategoryCommand
{
    public record CreateCategoryCommand : IRequest<Result<CategoryDto>>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;
        private readonly CreateCategoryCommandValidator _validator;

        public CreateCategoryCommandHandler(
            DatabaseContext context, 
            IUserSession userSession,
            CreateCategoryCommandValidator validator)
        {
            _context = context;
            _userSession = userSession;
            _validator = validator;
        }

        public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            // Validate command using validator
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<CategoryDto>(validationResult.Error);
            }

            try
            {
                var category = Domain.Models.Category.Create(
                    request.Name,
                    request.Description,
                    request.ImageUrl,
                    _userSession.UserName ?? "System"
                );

                _context.Categories.Add(category);
                await _context.SaveChangesAsync(cancellationToken);

                var categoryDto = new CategoryDto
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    IsActive = category.IsActive,
                    VehicleCount = 0
                };

                return Result.Success(categoryDto);
            }
            catch (System.Exception ex)
            {
                return Result.Failure<CategoryDto>($"Error creating category: {ex.Message}");
            }
        }
    }
}

