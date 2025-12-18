using Application.Common;
using Application.Features.Customer.DTOs;
using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Query.GetAllCustomersQuery
{
    public record GetAllCustomersQuery : IRequest<Result<PagedResult<CustomerDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public Domain.Enums.CustomerState? State { get; set; }
    }

    public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, Result<PagedResult<CustomerDto>>>
    {
        private readonly DatabaseContext _context;

        public GetAllCustomersQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<CustomerDto>>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Customers
                .Include(c => c.User)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(c => c.UserName.ToLower().Contains(searchTerm) ||
                                       c.MobileNumber.Contains(searchTerm) ||
                                       c.NationalNumber.Contains(searchTerm) ||
                                       (c.User != null && c.User.Email != null && c.User.Email.ToLower().Contains(searchTerm)));
            }

            if (request.State.HasValue)
            {
                query = query.Where(c => c.State == request.State.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CustomerDto
                {
                    CustomerId = c.CustomerId,
                    MobileNumber = c.MobileNumber,
                    UserName = c.UserName,
                    NationalNumber = c.NationalNumber,
                    Gender = c.Gender,
                    State = c.State,
                    Email = c.User != null ? c.User.Email : null,
                    CreatedDate = c.CreatedDate
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<CustomerDto>
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

