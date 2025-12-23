using Application.Features.SubCategory.DTOs;
using CSharpFunctionalExtensions;
using Domain.Common;
using Domain.Models;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.SubCategory.Command.CreateSubCategoryCommand
{
    public record CreateSubCategoryCommand : IRequest<Result<SubCategoryDto>>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public bool IsOffer { get; set; } = false;
        public string? ImageUrl { get; set; }
    }

    public class CreateSubCategoryCommandHandler : IRequestHandler<CreateSubCategoryCommand, Result<SubCategoryDto>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;
        private readonly CreateSubCategoryCommandValidator _validator;

        public CreateSubCategoryCommandHandler(
            DatabaseContext context, 
            IUserSession userSession,
            CreateSubCategoryCommandValidator validator)
        {
            _context = context;
            _userSession = userSession;
            _validator = validator;
        }

        public async Task<Result<SubCategoryDto>> Handle(CreateSubCategoryCommand request, CancellationToken cancellationToken)
        {
            // Validate command using validator
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<SubCategoryDto>(validationResult.Error);
            }

            // Verify category exists
            var category = await _context.Categories
                .Include(c => c.City)
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId && c.IsActive, cancellationToken);

            if (category == null)
            {
                return Result.Failure<SubCategoryDto>($"Category with ID {request.CategoryId} not found");
            }

            try
            {
                var subCategory = Domain.Models.SubCategory.Create(
                    request.Name,
                    request.Description,
                    request.CategoryId,
                    request.Price,
                    request.ImageUrl,
                    request.IsOffer,
                    _userSession.UserName ?? "System"
                );

                _context.SubCategories.Add(subCategory);
                await _context.SaveChangesAsync(cancellationToken);

                var subCategoryDto = new SubCategoryDto
                {
                    SubCategoryId = subCategory.SubCategoryId,
                    Name = subCategory.Name,
                    Description = subCategory.Description,
                    ImageUrl = subCategory.ImageUrl,
                    IsActive = subCategory.IsActive,
                    IsOffer = subCategory.IsOffer,
                    Price = subCategory.Price,
                    CategoryId = subCategory.CategoryId,
                    CategoryName = category.Name,
                    CityId = category.CityId,
                    CityName = category.City.Name,
                    VehicleCount = 0
                };

                return Result.Success(subCategoryDto);
            }
            catch (System.Exception ex)
            {
                return Result.Failure<SubCategoryDto>($"Error creating subcategory: {ex.Message}");
            }
        }
    }
}

