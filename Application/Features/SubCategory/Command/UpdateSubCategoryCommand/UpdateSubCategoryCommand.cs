using Application.Features.SubCategory.DTOs;
using CSharpFunctionalExtensions;
using Domain.Common;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.SubCategory.Command.UpdateSubCategoryCommand
{
    public record UpdateSubCategoryCommand : IRequest<Result<SubCategoryDto>>
    {
        public int SubCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public bool IsOffer { get; set; } = false;
        public string? ImageUrl { get; set; }
    }

    public class UpdateSubCategoryCommandHandler : IRequestHandler<UpdateSubCategoryCommand, Result<SubCategoryDto>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;
        private readonly UpdateSubCategoryCommandValidator _validator;

        public UpdateSubCategoryCommandHandler(
            DatabaseContext context, 
            IUserSession userSession,
            UpdateSubCategoryCommandValidator validator)
        {
            _context = context;
            _userSession = userSession;
            _validator = validator;
        }

        public async Task<Result<SubCategoryDto>> Handle(UpdateSubCategoryCommand request, CancellationToken cancellationToken)
        {
            // Validate command using validator
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<SubCategoryDto>(validationResult.Error);
            }

            var subCategory = await _context.SubCategories
                .Include(sc => sc.Category)
                    .ThenInclude(c => c.City)
                .FirstOrDefaultAsync(sc => sc.SubCategoryId == request.SubCategoryId, cancellationToken);

            if (subCategory == null)
            {
                return Result.Failure<SubCategoryDto>($"SubCategory with ID {request.SubCategoryId} not found");
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
                subCategory.Update(
                    request.Name,
                    request.Description,
                    request.CategoryId,
                    request.Price,
                    request.ImageUrl,
                    request.IsOffer,
                    _userSession.UserName ?? "System"
                );

                await _context.SaveChangesAsync(cancellationToken);

                var vehicleCount = await _context.Vehicles
                    .CountAsync(v => v.SubCategoryId == subCategory.SubCategoryId, cancellationToken);

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
                    CategoryName = subCategory.Category.Name,
                    CityId = subCategory.Category.CityId,
                    CityName = subCategory.Category.City.Name,
                    VehicleCount = vehicleCount
                };

                return Result.Success(subCategoryDto);
            }
            catch (System.Exception ex)
            {
                return Result.Failure<SubCategoryDto>($"Error updating subcategory: {ex.Message}");
            }
        }
    }
}

