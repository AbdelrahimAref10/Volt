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

namespace Application.Features.Order.Query.Reports.CancellationFeesReportQuery
{
    public record CancellationFeesReportQuery : IRequest<Result<List<CustomerWalletDto>>>
    {
    }

    public class CancellationFeesReportQueryHandler : IRequestHandler<CancellationFeesReportQuery, Result<List<CustomerWalletDto>>>
    {
        private readonly DatabaseContext _context;

        public CancellationFeesReportQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CustomerWalletDto>>> Handle(CancellationFeesReportQuery request, CancellationToken cancellationToken)
        {
            var cancellationFees = await _context.CustomerWallets
                .Where(cw => cw.Type == WalletType.OrderCancellationFees)
                .OrderByDescending(cw => cw.CreatedDate)
                .Select(cw => new CustomerWalletDto
                {
                    Id = cw.Id,
                    CustomerId = cw.CustomerId,
                    OrderId = cw.OrderId,
                    Withdraw = cw.Withdraw,
                    Deposit = cw.Deposit,
                    Description = cw.Description,
                    Type = cw.Type,
                    State = cw.State,
                    CreatedDate = cw.CreatedDate
                })
                .ToListAsync(cancellationToken);

            return Result.Success(cancellationFees);
        }
    }
}

