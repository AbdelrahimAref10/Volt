using CSharpFunctionalExtensions;
using Domain.Common;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Category.Command.DeleteCategoryCommand
{
    public record DeleteCategoryCommand : IRequest<Result<bool>>
    {
        public int CategoryId { get; set; }
    }

    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<bool>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;

        public DeleteCategoryCommandHandler(DatabaseContext context, IUserSession userSession)
        {
            _context = context;
            _userSession = userSession;
        }

        public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _context.Categories
                .Include(c => c.Vehicles)
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId, cancellationToken);

            if (category == null)
            {
                return Result.Failure<bool>($"Category with ID {request.CategoryId} not found");
            }

            // Check if category has vehicles
            if (category.Vehicles.Any())
            {
                return Result.Failure<bool>("Cannot delete category that has vehicles. Please remove or reassign vehicles first.");
            }

            try
            {
                category.Deactivate(_userSession.UserName ?? "System");
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success(true);
            }
            catch (System.Exception ex)
            {
                return Result.Failure<bool>($"Error deleting category: {ex.Message}");
            }
        }
    }
}


