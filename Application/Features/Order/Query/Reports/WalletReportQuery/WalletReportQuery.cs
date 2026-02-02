using Application.Features.Order.DTOs;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Query.Reports.WalletReportQuery
{
    public record WalletReportQuery : IRequest<Result<List<WalletReportEntryDto>>>
    {
        /// <summary>Filter by exact customer ID.</summary>
        public int? CustomerId { get; set; }

        /// <summary>Search by customer name or mobile number (contains, case-insensitive).</summary>
        public string? CustomerSearch { get; set; }

        /// <summary>Filter by wallet entry state (Pending, Paid).</summary>
        public CustomerWalletState? State { get; set; }
    }

    public class WalletReportQueryHandler : IRequestHandler<WalletReportQuery, Result<List<WalletReportEntryDto>>>
    {
        private readonly DatabaseContext _context;

        public WalletReportQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<List<WalletReportEntryDto>>> Handle(WalletReportQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CustomerWallets.AsQueryable();

            if (request.CustomerId.HasValue)
            {
                query = query.Where(cw => cw.CustomerId == request.CustomerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.CustomerSearch))
            {
                var search = request.CustomerSearch.Trim().ToLower();
                query = query.Where(cw =>
                    cw.Customer.FullName.ToLower().Contains(search) ||
                    cw.Customer.MobileNumber.ToLower().Contains(search));
            }

            if (request.State.HasValue)
            {
                query = query.Where(cw => cw.State == request.State.Value);
            }

            var list = await query
                .OrderByDescending(cw => cw.CreatedDate)
                .Select(cw => new WalletReportEntryDto
                {
                    Id = cw.Id,
                    CustomerId = cw.CustomerId,
                    CustomerName = cw.Customer.FullName,
                    CustomerMobileNumber = cw.Customer.MobileNumber,
                    OrderId = cw.OrderId,
                    Withdraw = cw.Withdraw,
                    Deposit = cw.Deposit,
                    Description = cw.Description,
                    Type = cw.Type,
                    State = cw.State,
                    CreatedDate = cw.CreatedDate
                })
                .ToListAsync(cancellationToken);

            return Result.Success(list);
        }
    }
}
