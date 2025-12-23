using Application.Common;
using Application.Features.SubCategory.DTOs;
using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.SubCategory.Query.GetAllSubCategoriesQuery
{
    public record GetAllSubCategoriesQuery : IRequest<Result<PagedResult<SubCategoryDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int? CategoryId { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class GetAllSubCategoriesQueryHandler : IRequestHandler<GetAllSubCategoriesQuery, Result<PagedResult<SubCategoryDto>>>
    {
        private readonly DatabaseContext _context;

        public GetAllSubCategoriesQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<SubCategoryDto>>> Handle(GetAllSubCategoriesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.SubCategories
                .Include(sc => sc.Category)
                    .ThenInclude(c => c.City)
                .Where(sc => sc.IsActive)
                .AsQueryable();

            // Apply filters
            if (request.CategoryId.HasValue)
            {
                query = query.Where(sc => sc.CategoryId == request.CategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(sc => sc.Name.ToLower().Contains(searchTerm) || 
                                       sc.Description.ToLower().Contains(searchTerm) ||
                                       sc.Category.Name.ToLower().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(sc => sc.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(sc => new SubCategoryDto
                {
                    SubCategoryId = sc.SubCategoryId,
                    Name = sc.Name,
                    Description = sc.Description,
                    ImageUrl = sc.ImageUrl,
                    IsActive = sc.IsActive,
                    IsOffer = sc.IsOffer,
                    Price = sc.Price,
                    CategoryId = sc.CategoryId,
                    CategoryName = sc.Category.Name,
                    CityId = sc.Category.CityId,
                    CityName = sc.Category.City.Name,
                    VehicleCount = sc.Vehicles.Count
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<SubCategoryDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(result);
        }
    }
}

