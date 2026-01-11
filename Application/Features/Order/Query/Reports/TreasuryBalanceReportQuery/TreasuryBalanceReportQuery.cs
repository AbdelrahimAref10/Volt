using Application.Features.Order.DTOs;
using CSharpFunctionalExtensions;
using Domain.Models;
using Domain.Services;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Query.Reports.TreasuryBalanceReportQuery
{
    public record TreasuryBalanceReportQuery : IRequest<Result<TreasuryReportDto>>
    {
    }

    public class TreasuryBalanceReportQueryHandler : IRequestHandler<TreasuryBalanceReportQuery, Result<TreasuryReportDto>>
    {
        private readonly DatabaseContext _context;

        public TreasuryBalanceReportQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<TreasuryReportDto>> Handle(TreasuryBalanceReportQuery request, CancellationToken cancellationToken)
        {
            var treasury = await _context.CompanyTreasuries.FirstOrDefaultAsync(cancellationToken);

            if (treasury == null)
            {
                // Create initial treasury if it doesn't exist
                treasury = CompanyTreasury.Create("System");
                _context.CompanyTreasuries.Add(treasury);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var balance = TreasuryService.GetTreasuryBalance(treasury);

            var report = new TreasuryReportDto
            {
                TotalRevenue = treasury.TotalRevenue,
                TotalCancellationFees = treasury.TotalCancellationFees,
                Balance = balance,
                LastUpdated = treasury.LastUpdated
            };

            return Result.Success(report);
        }
    }
}

