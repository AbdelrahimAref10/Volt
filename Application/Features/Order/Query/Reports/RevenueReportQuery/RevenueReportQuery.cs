using Application.Features.Order.DTOs;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Query.Reports.RevenueReportQuery
{
    public record RevenueReportQuery : IRequest<Result<RevenueReportDto>>
    {
        public string Period { get; set; } = "month"; // "month", "quarter", "year"
    }

    public class RevenueReportQueryHandler : IRequestHandler<RevenueReportQuery, Result<RevenueReportDto>>
    {
        private readonly DatabaseContext _context;

        public RevenueReportQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<RevenueReportDto>> Handle(RevenueReportQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            DateTime startDate;

            switch (request.Period.ToLower())
            {
                case "month":
                    startDate = new DateTime(now.Year, now.Month, 1);
                    break;
                case "quarter":
                    var quarter = (now.Month - 1) / 3;
                    startDate = new DateTime(now.Year, quarter * 3 + 1, 1);
                    break;
                case "year":
                    startDate = new DateTime(now.Year, 1, 1);
                    break;
                default:
                    return Result.Failure<RevenueReportDto>("Invalid period. Must be 'month', 'quarter', or 'year'");
            }

            var completedOrders = await _context.Orders
                .Include(o => o.OrderPayments)
                .Where(o => o.OrderState == OrderState.Completed
                    && o.CreatedDate >= startDate
                    && o.CreatedDate <= now)
                .ToListAsync(cancellationToken);

            var totalRevenue = completedOrders
                .SelectMany(o => o.OrderPayments)
                .Where(op => op.State == PaymentState.Paid)
                .Sum(op => op.Total);

            var totalOrders = completedOrders.Count;
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var report = new RevenueReportDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AverageOrderValue = averageOrderValue,
                Period = request.Period
            };

            return Result.Success(report);
        }
    }
}

