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
        public string VehicleCode { get; set; } = string.Empty;
        public int SubCategoryId { get; set; }
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
                .Include(v => v.SubCategory)
                    .ThenInclude(sc => sc.Category)
                        .ThenInclude(c => c.City)
                .FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId, cancellationToken);

            if (vehicle == null)
            {
                return Result.Failure<VehicleDto>($"Vehicle with ID {request.VehicleId} not found");
            }

            // Verify subcategory exists
            var subCategory = await _context.SubCategories
                .Include(sc => sc.Category)
                    .ThenInclude(c => c.City)
                .FirstOrDefaultAsync(sc => sc.SubCategoryId == request.SubCategoryId && sc.IsActive, cancellationToken);

            if (subCategory == null)
            {
                return Result.Failure<VehicleDto>($"SubCategory with ID {request.SubCategoryId} not found");
            }

            try
            {
                vehicle.Update(
                    request.Name,
                    request.VehicleCode,
                    request.SubCategoryId,
                    request.Status,
                    request.ImageUrl,
                    _userSession.UserName ?? "System"
                );

                await _context.SaveChangesAsync(cancellationToken);

                var vehicleDto = new VehicleDto
                {
                    VehicleId = vehicle.VehicleId,
                    Name = vehicle.Name,
                    VehicleCode = vehicle.VehicleCode,
                    ImageUrl = vehicle.ImageUrl,
                    Status = vehicle.Status,
                    SubCategoryId = vehicle.SubCategoryId,
                    SubCategoryName = vehicle.SubCategory.Name,
                    SubCategoryPrice = vehicle.SubCategory.Price,
                    CategoryId = vehicle.SubCategory.CategoryId,
                    CategoryName = vehicle.SubCategory.Category.Name,
                    CityId = vehicle.SubCategory.Category.CityId,
                    CityName = vehicle.SubCategory.Category.City.Name,
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


