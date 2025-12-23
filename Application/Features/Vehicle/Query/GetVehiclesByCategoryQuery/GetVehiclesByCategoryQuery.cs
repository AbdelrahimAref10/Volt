using Application.Common;
using Application.Features.Vehicle.DTOs;
using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Vehicle.Query.GetVehiclesByCategoryQuery
{
    public record GetVehiclesByCategoryQuery : IRequest<Result<PagedResult<VehicleDto>>>
    {
        public int CategoryId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class GetVehiclesByCategoryQueryHandler : IRequestHandler<GetVehiclesByCategoryQuery, Result<PagedResult<VehicleDto>>>
    {
        private readonly DatabaseContext _context;

        public GetVehiclesByCategoryQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<VehicleDto>>> Handle(GetVehiclesByCategoryQuery request, CancellationToken cancellationToken)
        {
            // Verify category exists
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryId == request.CategoryId && c.IsActive, cancellationToken);

            if (!categoryExists)
            {
                return Result.Failure<PagedResult<VehicleDto>>($"Category with ID {request.CategoryId} not found");
            }

            var query = _context.Vehicles
                .Include(v => v.SubCategory)
                    .ThenInclude(sc => sc.Category)
                        .ThenInclude(c => c.City)
                .Where(v => v.SubCategory.CategoryId == request.CategoryId);

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


