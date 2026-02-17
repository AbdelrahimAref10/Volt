using CSharpFunctionalExtensions;
using Domain.Common;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Command.ActivateCustomerCommand
{
    public class ActivateCustomerCommandValidator
    {
        private readonly DatabaseContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ActivateCustomerCommandValidator(DatabaseContext context, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result> ValidateAsync(ActivateCustomerCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.MobileNumber))
            {
                return Result.Failure("Mobile number is required");
            }

            if (string.IsNullOrWhiteSpace(request.InvitationCode))
            {
                return Result.Failure("Invitation code is required");
            }

            var customer = await _context.Customers.AsTracking()
                .FirstOrDefaultAsync(c => c.MobileNumber == request.MobileNumber, cancellationToken);

            if (customer == null)
            {
                return Result.Failure("Customer not found");
            }

            if (customer.State == Domain.Enums.CustomerState.Active)
            {
                return Result.Failure("Customer is already activated");
            }

            // Validate invitation code
            if (!customer.ValidateInvitationCode(request.InvitationCode, _dateTimeProvider))
            {
                return Result.Failure("Invalid or expired invitation code");
            }

            return Result.Success();
        }
    }
}

