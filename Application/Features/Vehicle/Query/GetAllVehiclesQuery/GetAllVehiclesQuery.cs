using Application.Common;
using Application.Features.Vehicle.DTOs;
using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Vehicle.Query.GetAllVehiclesQuery
{
    public record GetAllVehiclesQuery : IRequest<Result<PagedResult<VehicleDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public string? Status { get; set; }
    }

    public class GetAllVehiclesQueryHandler : IRequestHandler<GetAllVehiclesQuery, Result<PagedResult<VehicleDto>>>
    {
        private readonly DatabaseContext _context;

        public GetAllVehiclesQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<VehicleDto>>> Handle(GetAllVehiclesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Vehicles
                .Include(v => v.SubCategory)
                    .ThenInclude(sc => sc.Category)
                        .ThenInclude(c => c.City)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(v => v.Name.ToLower().Contains(searchTerm) || 
                                       v.SubCategory.Name.ToLower().Contains(searchTerm) ||
                                       v.SubCategory.Category.Name.ToLower().Contains(searchTerm));
            }

            if (request.CategoryId.HasValue)
            {
                query = query.Where(v => v.SubCategory.CategoryId == request.CategoryId.Value);
            }

            if (request.SubCategoryId.HasValue)
            {
                query = query.Where(v => v.SubCategoryId == request.SubCategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(v => v.Status == request.Status);
            }

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


