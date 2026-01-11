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

namespace Application.Features.Order.Query.Reports.OrdersByStateReportQuery
{
    public record OrdersByStateReportQuery : IRequest<Result<List<OrdersByStateReportDto>>>
    {
    }

    public class OrdersByStateReportQueryHandler : IRequestHandler<OrdersByStateReportQuery, Result<List<OrdersByStateReportDto>>>
    {
        private readonly DatabaseContext _context;

        public OrdersByStateReportQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<List<OrdersByStateReportDto>>> Handle(OrdersByStateReportQuery request, CancellationToken cancellationToken)
        {
            var report = await _context.Orders
                .GroupBy(o => o.OrderState)
                .Select(g => new OrdersByStateReportDto
                {
                    OrderState = g.Key,
                    OrderStateName = g.Key.ToString(),
                    Count = g.Count()
                })
                .OrderBy(r => r.OrderState)
                .ToListAsync(cancellationToken);

            return Result.Success(report);
        }
    }
}

