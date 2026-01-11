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
    public record CancellationFeesReportQuery : IRequest<Result<List<OrderCancellationFeeDto>>>
    {
    }

    public class CancellationFeesReportQueryHandler : IRequestHandler<CancellationFeesReportQuery, Result<List<OrderCancellationFeeDto>>>
    {
        private readonly DatabaseContext _context;

        public CancellationFeesReportQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<List<OrderCancellationFeeDto>>> Handle(CancellationFeesReportQuery request, CancellationToken cancellationToken)
        {
            var cancellationFees = await _context.OrderCancellationFees
                .OrderByDescending(cf => cf.CreatedDate)
                .Select(cf => new OrderCancellationFeeDto
                {
                    Id = cf.Id,
                    CustomerId = cf.CustomerId,
                    OrderId = cf.OrderId,
                    Amount = cf.Amount,
                    State = cf.State,
                    CreatedDate = cf.CreatedDate
                })
                .ToListAsync(cancellationToken);

            return Result.Success(cancellationFees);
        }
    }
}

