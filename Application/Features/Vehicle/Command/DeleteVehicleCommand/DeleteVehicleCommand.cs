using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Vehicle.Command.DeleteVehicleCommand
{
    public record DeleteVehicleCommand : IRequest<Result<bool>>
    {
        public int VehicleId { get; set; }
    }

    public class DeleteVehicleCommandHandler : IRequestHandler<DeleteVehicleCommand, Result<bool>>
    {
        private readonly DatabaseContext _context;

        public DeleteVehicleCommandHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
        {
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId, cancellationToken);

            if (vehicle == null)
            {
                return Result.Failure<bool>($"Vehicle with ID {request.VehicleId} not found");
            }

            try
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success(true);
            }
            catch (System.Exception ex)
            {
                return Result.Failure<bool>($"Error deleting vehicle: {ex.Message}");
            }
        }
    }
}


