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
            // Count cancelled orders (orders with cancellation fees)
            var totalCancelledOrders = await _context.OrderCancellationFees
                .CountAsync(cancellationToken);

            var cancellationFees = await _context.OrderCancellationFees
                .ToListAsync(cancellationToken);

            var totalCancellationFees = cancellationFees.Sum(cf => cf.Amount);
            var paidCancellationFees = cancellationFees
                .Where(cf => cf.State == CancellationFeeState.Paid)
                .Sum(cf => cf.Amount);
            var unpaidCancellationFees = cancellationFees
                .Where(cf => cf.State == CancellationFeeState.NotYet)
                .Sum(cf => cf.Amount);

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

