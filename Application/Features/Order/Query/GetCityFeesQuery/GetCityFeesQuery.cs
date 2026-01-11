using Application.Features.Order.DTOs;
using CSharpFunctionalExtensions;
using Domain.Common;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Query.GetCityFeesQuery
{
    public record GetCityFeesQuery : IRequest<Result<CityFeesDto>>
    {
        // No parameters needed - will get from logged-in customer
    }

    public class GetCityFeesQueryHandler : IRequestHandler<GetCityFeesQuery, Result<CityFeesDto>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;

        public GetCityFeesQueryHandler(DatabaseContext context, IUserSession userSession)
        {
            _context = context;
            _userSession = userSession;
        }

        public async Task<Result<CityFeesDto>> Handle(GetCityFeesQuery request, CancellationToken cancellationToken)
        {
            // Get customer ID from session (CustomerId is stored as NameIdentifier in JWT for customers)
            var customerId = _userSession.UserId;

            if (customerId <= 0)
            {
                return Result.Failure<CityFeesDto>("Customer not found or not authenticated");
            }

            var customer = await _context.Customers
                .Include(c => c.City)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

            if (customer == null)
            {
                return Result.Failure<CityFeesDto>("Customer not found");
            }

            if (customer.City == null)
            {
                return Result.Failure<CityFeesDto>("Customer city not found");
            }

            var cityFeesDto = new CityFeesDto
            {
                CityId = customer.City.CityId,
                ServiceFees = customer.City.ServiceFees,
                DeliveryFees = customer.City.DeliveryFees,
                UrgentFees = customer.City.UrgentDelivery,
                CancellationFees = customer.City.CancellationFees
            };

            return Result.Success(cityFeesDto);
        }
    }
}

