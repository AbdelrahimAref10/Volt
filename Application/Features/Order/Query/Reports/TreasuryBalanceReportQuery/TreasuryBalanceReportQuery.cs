using Application.Features.Order.DTOs;
using CSharpFunctionalExtensions;
using Domain.Models;
using Domain.Services;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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
            var treasuryRecords = await _context.CompanyTreasuries.ToListAsync(cancellationToken);

            // Calculate totals from all treasury records
            var totalDebit = treasuryRecords.Sum(t => t.DebitAmount);
            var totalCredit = treasuryRecords.Sum(t => t.CreditAmount);
            var balance = totalCredit - totalDebit; // Credit (money in) - Debit (money out)

            var report = new TreasuryReportDto
            {
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                Balance = balance,
                LastUpdated = treasuryRecords.Any() ? treasuryRecords.Max(t => t.CreatedDate) : DateTime.UtcNow
            };

            return Result.Success(report);
        }
    }
}

