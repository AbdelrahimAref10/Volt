using Application.Common;
using Application.Features.Vehicle.DTOs;
using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Vehicle.Query.GetVehiclesBySubCategoryQuery
{
    public record GetVehiclesBySubCategoryQuery : IRequest<Result<PagedResult<VehicleDto>>>
    {
        public int SubCategoryId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class GetVehiclesBySubCategoryQueryHandler : IRequestHandler<GetVehiclesBySubCategoryQuery, Result<PagedResult<VehicleDto>>>
    {
        private readonly DatabaseContext _context;

        public GetVehiclesBySubCategoryQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<VehicleDto>>> Handle(GetVehiclesBySubCategoryQuery request, CancellationToken cancellationToken)
        {
            // Verify subcategory exists
            var subCategoryExists = await _context.SubCategories
                .AnyAsync(sc => sc.SubCategoryId == request.SubCategoryId && sc.IsActive, cancellationToken);

            if (!subCategoryExists)
            {
                return Result.Failure<PagedResult<VehicleDto>>($"SubCategory with ID {request.SubCategoryId} not found");
            }

            var query = _context.Vehicles
                .Include(v => v.SubCategory)
                    .ThenInclude(sc => sc.Category)
                        .ThenInclude(c => c.City)
                .Where(v => v.SubCategoryId == request.SubCategoryId);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(v => v.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(v => new VehicleDto
                {
                    VehicleId = v.VehicleId,
                    Name = v.Name,
                    ImageUrl = v.ImageUrl,
                    Status = v.Status,
                    SubCategoryId = v.SubCategoryId,
                    SubCategoryName = v.SubCategory.Name,
                    SubCategoryPrice = v.SubCategory.Price,
                    CategoryId = v.SubCategory.CategoryId,
                    CategoryName = v.SubCategory.Category.Name,
                    CityId = v.SubCategory.Category.CityId,
                    CityName = v.SubCategory.Category.City.Name,
                    IsNewThisMonth = v.IsNewThisMonth
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<VehicleDto>
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

