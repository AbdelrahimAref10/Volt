using Application.Features.Order.DTOs;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Query.Reports.CancellationReportQuery
{
    public record CancellationReportQuery : IRequest<Result<CancellationReportDto>>
    {
    }

    public class CancellationReportQueryHandler : IRequestHandler<CancellationReportQuery, Result<CancellationReportDto>>
    {
        private readonly DatabaseContext _context;

        public CancellationReportQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<CancellationReportDto>> Handle(CancellationReportQuery request, CancellationToken cancellationToken)
        {
            var cancellationWalletEntries = await _context.CustomerWallets
                .Where(cw => cw.Type == WalletType.OrderCancellationFees)
                .ToListAsync(cancellationToken);

            var totalCancelledOrders = cancellationWalletEntries.Count;
            var totalCancellationFees = cancellationWalletEntries.Sum(cw => cw.Withdraw);
            var paidCancellationFees = cancellationWalletEntries.Where(cw => cw.State == CustomerWalletState.Paid).Sum(cw => cw.Withdraw);
            var unpaidCancellationFees = cancellationWalletEntries.Where(cw => cw.State == CustomerWalletState.Pending).Sum(cw => cw.Withdraw);

            var report = new CancellationReportDto
            {
                TotalCancelledOrders = totalCancelledOrders,
                TotalCancellationFees = totalCancellationFees,
                PaidCancellationFees = paidCancellationFees,
                UnpaidCancellationFees = unpaidCancellationFees
            };

            return Result.Success(report);
        }
    }
}

