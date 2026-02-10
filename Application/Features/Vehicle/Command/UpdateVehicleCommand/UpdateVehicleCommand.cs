using CSharpFunctionalExtensions;
using Domain.Common;
using Infrastructure;
using Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Vehicle.Command.UpdateVehicleCommand
{
    public record UpdateVehicleCommand : IRequest<Result<int>>
    {
        public int VehicleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string VehicleCode { get; set; } = string.Empty;
        public int SubCategoryId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, Result<int>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;
        private readonly IImageService _imageService;

        public UpdateVehicleCommandHandler(DatabaseContext context, IUserSession userSession, IImageService imageService)
        {
            _context = context;
            _userSession = userSession;
            _imageService = imageService;
        }

        public async Task<Result<int>> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
        {
            var vehicle = await _context.Vehicles
                .AsTracking()
                .FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId, cancellationToken);

            if (vehicle == null)
            {
                return Result.Failure<int>($"Vehicle with ID {request.VehicleId} not found");
            }

            // Verify subcategory exists
            var subCategory = await _context.SubCategories
                .FirstOrDefaultAsync(sc => sc.SubCategoryId == request.SubCategoryId && sc.IsActive, cancellationToken);

            if (subCategory == null)
            {
                return Result.Failure<int>($"SubCategory with ID {request.SubCategoryId} not found");
            }

            // Store old image URL for deletion if new image is provided
            string? oldImageUrl = vehicle.ImageUrl;

            // Save base64 image as file and get URL
            string? imageUrl = vehicle.ImageUrl; // Keep existing if no new image provided
            if (!string.IsNullOrWhiteSpace(request.ImageUrl))
            {
                imageUrl = _imageService.SaveBase64Image(request.ImageUrl, "vehicles");
                // Delete old image if it exists and is different
                if (!string.IsNullOrWhiteSpace(oldImageUrl) && oldImageUrl != imageUrl)
                {
                    _imageService.DeleteImage(oldImageUrl);
                }
            }

            vehicle.Update(
                request.Name,
                request.VehicleCode,
                request.SubCategoryId,
                request.Status,
                imageUrl,
                _userSession.UserName ?? "System"
            );

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(vehicle.VehicleId);
        }
    }
}


