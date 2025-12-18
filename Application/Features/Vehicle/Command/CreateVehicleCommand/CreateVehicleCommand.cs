using Application.Features.Vehicle.DTOs;
using CSharpFunctionalExtensions;
using Domain.Common;
using Domain.Models;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Vehicle.Command.CreateVehicleCommand
{
    public record CreateVehicleCommand : IRequest<Result<VehicleDto>>
    {
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal PricePerHour { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, Result<VehicleDto>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;

        public CreateVehicleCommandHandler(DatabaseContext context, IUserSession userSession)
        {
            _context = context;
            _userSession = userSession;
        }

        public async Task<Result<VehicleDto>> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
        {
            // Verify category exists
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId && c.IsActive, cancellationToken);

            if (category == null)
            {
                return Result.Failure<VehicleDto>($"Category with ID {request.CategoryId} not found");
            }

            try
            {
                var vehicle = Domain.Models.Vehicle.Create(
                    request.Name,
                    request.CategoryId,
                    request.PricePerHour,
                    request.Status,
                    request.ImageUrl,
                    _userSession.UserName ?? "System"
                );

                _context.Vehicles.Add(vehicle);
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
                return Result.Failure<VehicleDto>($"Error creating vehicle: {ex.Message}");
            }
        }
    }
}

