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

namespace Application.Features.Order.Query.GetReservedVehiclePerSubCategoryQuery
{
    public record GetReservedVehiclePerSubCategoryQuery : IRequest<Result<List<ReservedDateDto>>>
    {
        public int SubCategoryId { get; set; }
    }

    public class GetReservedVehiclePerSubCategoryQueryHandler : IRequestHandler<GetReservedVehiclePerSubCategoryQuery, Result<List<ReservedDateDto>>>
    {
        private readonly DatabaseContext _context;

        public GetReservedVehiclePerSubCategoryQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ReservedDateDto>>> Handle(GetReservedVehiclePerSubCategoryQuery request, CancellationToken cancellationToken)
        {
            // Get all reserved vehicles for this subcategory where:
            // - State = StillBooked
            // - Order.OrderState != Completed && != Cancelled (orders that are still active)
            var reservedRecords = await _context.ReservedVehiclesPerDays
                .Include(rv => rv.Order)
                .Where(rv => rv.SubCategoryId == request.SubCategoryId
                    && rv.State == ReservedVehicleState.StillBooked
                    && rv.Order.OrderState != OrderState.Completed)
                .ToListAsync(cancellationToken);

            // Expand date ranges into individual dates
            var reservedDates = new HashSet<DateTime>();

            foreach (var record in reservedRecords)
            {
                var currentDate = record.DateFrom.Date;
                var endDate = record.DateTo.Date;

                while (currentDate <= endDate)
                {
                    reservedDates.Add(currentDate);
                    currentDate = currentDate.AddDays(1);
                }
            }

            var result = reservedDates
                .OrderBy(d => d)
                .Select(d => new ReservedDateDto { Date = d })
                .ToList();

            return Result.Success(result);
        }
    }
}

