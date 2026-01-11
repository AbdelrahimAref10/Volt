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

namespace Application.Features.Order.Query.Reports.VehicleUtilizationReportQuery
{
    public record VehicleUtilizationReportQuery : IRequest<Result<List<VehicleUtilizationReportDto>>>
    {
    }

    public class VehicleUtilizationReportQueryHandler : IRequestHandler<VehicleUtilizationReportQuery, Result<List<VehicleUtilizationReportDto>>>
    {
        private readonly DatabaseContext _context;

        public VehicleUtilizationReportQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<List<VehicleUtilizationReportDto>>> Handle(VehicleUtilizationReportQuery request, CancellationToken cancellationToken)
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.SubCategory)
                .ToListAsync(cancellationToken);

            var reservedRecords = await _context.ReservedVehiclesPerDays
                .Where(rv => rv.State == ReservedVehicleState.StillBooked)
                .ToListAsync(cancellationToken);

            var report = new List<VehicleUtilizationReportDto>();

            foreach (var vehicle in vehicles)
            {
                var vehicleReservations = reservedRecords
                    .Where(rv => rv.VehicleId == vehicle.VehicleId)
                    .ToList();

                var totalDaysBooked = vehicleReservations
                    .Sum(rv => (rv.DateTo - rv.DateFrom).Days + 1);

                var totalOrders = vehicleReservations
                    .Select(rv => rv.OrderId)
                    .Distinct()
                    .Count();

                // Calculate utilization percentage (assuming 365 days in a year)
                // This is a simplified calculation - you might want to adjust based on your business logic
                var daysSinceCreation = vehicle.CreatedDate != default 
                    ? (DateTime.UtcNow - vehicle.CreatedDate).TotalDays 
                    : 365;
                var utilizationPercentage = daysSinceCreation > 0 
                    ? (decimal)(totalDaysBooked / daysSinceCreation * 100) 
                    : 0;

                report.Add(new VehicleUtilizationReportDto
                {
                    VehicleId = vehicle.VehicleId,
                    VehicleName = vehicle.Name,
                    VehicleCode = vehicle.VehicleCode,
                    TotalDaysBooked = totalDaysBooked,
                    TotalOrders = totalOrders,
                    UtilizationPercentage = utilizationPercentage
                });
            }

            return Result.Success(report.OrderByDescending(r => r.TotalDaysBooked).ToList());
        }
    }
}

