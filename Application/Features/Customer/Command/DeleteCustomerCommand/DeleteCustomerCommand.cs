using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Command.DeleteCustomerCommand
{
    public record DeleteCustomerCommand : IRequest<Result<bool>>
    {
        public int CustomerId { get; set; }
    }

    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result<bool>>
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<Domain.Models.ApplicationUser> _userManager;

        public DeleteCustomerCommandHandler(
            DatabaseContext context,
            UserManager<Domain.Models.ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<Result<bool>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

            if (customer == null)
            {
                return Result.Failure<bool>("Customer not found");
            }

            // Delete the ApplicationUser (this will cascade delete the Customer due to OnDelete(DeleteBehavior.Cascade))
            if (customer.User != null)
            {
                var deleteResult = await _userManager.DeleteAsync(customer.User);
                if (!deleteResult.Succeeded)
                {
                    var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
                    return Result.Failure<bool>($"Failed to delete customer user: {errors}");
                }
            }
            else
            {
                // If no user, just delete the customer
                _context.Customers.Remove(customer);
            }

            var saveResult = await _context.SaveChangesAsyncWithResult(cancellationToken);
            if (!saveResult.IsSuccess)
            {
                return Result.Failure<bool>($"Failed to delete customer: {saveResult.ErrorMessage}");
            }

            return Result.Success(true);
        }
    }
}

