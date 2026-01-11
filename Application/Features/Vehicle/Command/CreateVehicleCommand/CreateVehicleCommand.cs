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
        public string VehicleCode { get; set; } = string.Empty;
        public int SubCategoryId { get; set; }
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
                var vehicle = Domain.Models.Vehicle.Create(
                    request.Name,
                    request.VehicleCode,
                    request.SubCategoryId,
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
                    VehicleCode = vehicle.VehicleCode,
                    ImageUrl = vehicle.ImageUrl,
                    Status = vehicle.Status,
                    SubCategoryId = vehicle.SubCategoryId,
                    SubCategoryName = subCategory.Name,
                    SubCategoryPrice = subCategory.Price,
                    CategoryId = subCategory.CategoryId,
                    CategoryName = subCategory.Category.Name,
                    CityId = subCategory.Category.CityId,
                    CityName = subCategory.Category.City.Name,
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

