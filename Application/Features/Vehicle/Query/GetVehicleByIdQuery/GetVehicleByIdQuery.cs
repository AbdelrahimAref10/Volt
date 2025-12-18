using Application.Features.Vehicle.DTOs;
using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Vehicle.Query.GetVehicleByIdQuery
{
    public record GetVehicleByIdQuery : IRequest<Result<VehicleDto>>
    {
        public int VehicleId { get; set; }
    }

    public class GetVehicleByIdQueryHandler : IRequestHandler<GetVehicleByIdQuery, Result<VehicleDto>>
    {
        private readonly DatabaseContext _context;

        public GetVehicleByIdQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<VehicleDto>> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Category)
                .FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId, cancellationToken);

            if (vehicle == null)
            {
                return Result.Failure<VehicleDto>($"Vehicle with ID {request.VehicleId} not found");
            }

            var vehicleDto = new VehicleDto
            {
                VehicleId = vehicle.VehicleId,
                Name = vehicle.Name,
                ImageUrl = vehicle.ImageUrl,
                PricePerHour = vehicle.PricePerHour,
                Status = vehicle.Status,
                CategoryId = vehicle.CategoryId,
                CategoryName = vehicle.Category.Name,
                IsNewThisMonth = vehicle.IsNewThisMonth
            };

            return Result.Success(vehicleDto);
        }
    }
}


