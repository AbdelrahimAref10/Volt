using Application.Common;
using Application.Features.Category.DTOs;
using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Category.Query.GetAllCategoriesQuery
{
    public record GetAllCategoriesQuery : IRequest<Result<PagedResult<CategoryDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, Result<PagedResult<CategoryDto>>>
    {
        private readonly DatabaseContext _context;

        public GetAllCategoriesQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<CategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    IsActive = c.IsActive,
                    VehicleCount = c.Vehicles.Count
                });

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(c => c.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<CategoryDto>
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


