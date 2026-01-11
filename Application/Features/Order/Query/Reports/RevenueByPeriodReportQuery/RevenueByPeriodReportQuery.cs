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

namespace Application.Features.Order.Query.Reports.RevenueByPeriodReportQuery
{
    public record RevenueByPeriodReportQuery : IRequest<Result<List<RevenueReportDto>>>
    {
        public string Period { get; set; } = "month"; // "month", "quarter", "year"
        public int NumberOfPeriods { get; set; } = 12; // Number of periods to return
    }

    public class RevenueByPeriodReportQueryHandler : IRequestHandler<RevenueByPeriodReportQuery, Result<List<RevenueReportDto>>>
    {
        private readonly DatabaseContext _context;

        public RevenueByPeriodReportQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<List<RevenueReportDto>>> Handle(RevenueByPeriodReportQuery request, CancellationToken cancellationToken)
        {
            var reports = new List<RevenueReportDto>();
            var now = DateTime.UtcNow;

            for (int i = request.NumberOfPeriods - 1; i >= 0; i--)
            {
                DateTime startDate;
                DateTime endDate;
                string periodLabel;

                switch (request.Period.ToLower())
                {
                    case "month":
                        var monthDate = now.AddMonths(-i);
                        startDate = new DateTime(monthDate.Year, monthDate.Month, 1);
                        endDate = startDate.AddMonths(1).AddDays(-1);
                        periodLabel = $"{monthDate:yyyy-MM}";
                        break;
                    case "quarter":
                        var quarterDate = now.AddMonths(-i * 3);
                        var quarter = (quarterDate.Month - 1) / 3;
                        startDate = new DateTime(quarterDate.Year, quarter * 3 + 1, 1);
                        endDate = startDate.AddMonths(3).AddDays(-1);
                        periodLabel = $"Q{quarter + 1} {quarterDate.Year}";
                        break;
                    case "year":
                        var yearDate = now.AddYears(-i);
                        startDate = new DateTime(yearDate.Year, 1, 1);
                        endDate = new DateTime(yearDate.Year, 12, 31);
                        periodLabel = yearDate.Year.ToString();
                        break;
                    default:
                        return Result.Failure<List<RevenueReportDto>>("Invalid period. Must be 'month', 'quarter', or 'year'");
                }

                var completedOrders = await _context.Orders
                    .Include(o => o.OrderPayments)
                    .Where(o => o.OrderState == OrderState.Completed
                        && o.CreatedDate >= startDate
                        && o.CreatedDate <= endDate)
                    .ToListAsync(cancellationToken);

                var totalRevenue = completedOrders
                    .SelectMany(o => o.OrderPayments)
                    .Where(op => op.State == PaymentState.Paid)
                    .Sum(op => op.Total);

                var totalOrders = completedOrders.Count;
                var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

                reports.Add(new RevenueReportDto
                {
                    TotalRevenue = totalRevenue,
                    TotalOrders = totalOrders,
                    AverageOrderValue = averageOrderValue,
                    Period = periodLabel
                });
            }

            return Result.Success(reports);
        }
    }
}

