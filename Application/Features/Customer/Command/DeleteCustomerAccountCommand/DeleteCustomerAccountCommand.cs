using CSharpFunctionalExtensions;
using Domain.Common;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Command.DeleteCustomerAccountCommand
{
    public record DeleteCustomerAccountCommand : IRequest<Result<bool>>;

    public class DeleteCustomerAccountCommandHandler : IRequestHandler<DeleteCustomerAccountCommand, Result<bool>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;
        private readonly IDateTimeProvider _dateTimeProvider;

        public DeleteCustomerAccountCommandHandler(
            DatabaseContext context,
            IUserSession userSession,
            IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _userSession = userSession;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<bool>> Handle(DeleteCustomerAccountCommand request, CancellationToken cancellationToken)
        {
            // Get customer ID from session
            var customerId = _userSession.UserId;
            if (customerId <= 0)
            {
                return Result.Failure<bool>("Customer not authenticated");
            }

            var customer = await _context.Customers
                .AsTracking()
                .Include(c => c.CustomerLocation)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

            if (customer == null)
            {
                return Result.Failure<bool>("Customer not found");
            }

            // Check if customer has any active orders
            var hasActiveOrders = await _context.Orders
                .AnyAsync(o => o.CustomerId == customerId && 
                             (o.OrderState == Domain.Enums.OrderState.Pending || 
                              o.OrderState == Domain.Enums.OrderState.Confirmed || 
                              o.OrderState == Domain.Enums.OrderState.OnWay), 
                         cancellationToken);

            if (hasActiveOrders)
            {
                return Result.Failure<bool>("Cannot delete account. You have active orders. Please complete or cancel them first.");
            }

            // Delete customer location if exists
            if (customer.CustomerLocation != null)
            {
                _context.CustomerLocations.Remove(customer.CustomerLocation);
            }

            // Delete customer
            _context.Customers.Remove(customer);

            var saveResult = await _context.SaveChangesAsyncWithResult(cancellationToken);
            if (!saveResult.IsSuccess)
            {
                return Result.Failure<bool>($"Failed to delete customer account: {saveResult.ErrorMessage}");
            }

            return Result.Success(true);
        }
    }
}

