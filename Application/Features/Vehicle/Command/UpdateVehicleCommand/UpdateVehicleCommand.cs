using Application.Features.Vehicle.DTOs;
using CSharpFunctionalExtensions;
using Domain.Common;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Vehicle.Command.UpdateVehicleCommand
{
    public record UpdateVehicleCommand : IRequest<Result<VehicleDto>>
    {
        public int VehicleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal PricePerHour { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, Result<VehicleDto>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;

        public UpdateVehicleCommandHandler(DatabaseContext context, IUserSession userSession)
        {
            _context = context;
            _userSession = userSession;
        }

        public async Task<Result<VehicleDto>> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Category)
                .FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId, cancellationToken);

            if (vehicle == null)
            {
                return Result.Failure<VehicleDto>($"Vehicle with ID {request.VehicleId} not found");
            }

            // Verify category exists
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId && c.IsActive, cancellationToken);

            if (category == null)
            {
                return Result.Failure<VehicleDto>($"Category with ID {request.CategoryId} not found");
            }

            try
            {
                vehicle.Update(
                    request.Name,
                    request.CategoryId,
                    request.PricePerHour,
                    request.Status,
                    request.ImageUrl,
                    _userSession.UserName ?? "System"
                );

                await _context.SaveChangesAsync(cancellationToken);

                var vehicleDto = new VehicleDto
                {
                    VehicleId = vehicle.VehicleId,
                    Name = vehicle.Name,
                    ImageUrl = vehicle.ImageUrl,
                    PricePerHour = vehicle.PricePerHour,
                    Status = vehicle.Status,
                    CategoryId = vehicle.CategoryId,
                    CategoryName = category.Name,
                    IsNewThisMonth = vehicle.IsNewThisMonth
                };

                return Result.Success(vehicleDto);
            }
            catch (System.Exception ex)
            {
                return Result.Failure<VehicleDto>($"Error updating vehicle: {ex.Message}");
            }
        }
    }
}


