using Application.Features.SubCategory.DTOs;
using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.SubCategory.Query.GetAllOffersByCityQuery
{
    public record GetAllOffersByCityQuery : IRequest<Result<List<SubCategoryDto>>>
    {
        public int CustomerId { get; set; }
    }

    public class GetAllOffersByCityQueryHandler : IRequestHandler<GetAllOffersByCityQuery, Result<List<SubCategoryDto>>>
    {
        private readonly DatabaseContext _context;

        public GetAllOffersByCityQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<List<SubCategoryDto>>> Handle(GetAllOffersByCityQuery request, CancellationToken cancellationToken)
        {
            // Get Customer to retrieve CityId
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

            if (customer == null)
            {
                return Result.Failure<List<SubCategoryDto>>("Customer not found");
            }

            var subCategories = await _context.SubCategories
                .Include(sc => sc.Category)
                    .ThenInclude(c => c.City)
                .Where(sc => sc.IsActive && sc.IsOffer && sc.Category.CityId == customer.CityId)
                .OrderBy(sc => sc.Name)
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

            return Result.Success(subCategories);
        }
    }
}

