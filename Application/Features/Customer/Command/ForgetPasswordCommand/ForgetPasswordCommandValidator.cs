using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Command.ForgetPasswordCommand
{
    public class ForgetPasswordCommandValidator
    {
        private readonly DatabaseContext _context;

        public ForgetPasswordCommandValidator(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result> ValidateAsync(ForgetPasswordCommand request, CancellationToken cancellationToken)
        {
            var hasPhone = !string.IsNullOrWhiteSpace(request.MobileNumber);
            var hasEmail = !string.IsNullOrWhiteSpace(request.Email);

            if (!hasPhone && !hasEmail)
            {
                return Result.Failure("Mobile number or email is required");
            }

            Domain.Models.Customer? customer = null;
            if (hasPhone)
            {
                customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.MobileNumber == request.MobileNumber, cancellationToken);
            }
            else if (hasEmail)
            {
                customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == request.Email, cancellationToken);
            }

            if (customer == null)
            {
                return Result.Failure("Customer not found");
            }

            if (customer.State != Domain.Enums.CustomerState.Active)
            {
                return Result.Failure("Account is not active. Please activate your account first.");
            }

            return Result.Success();
        }
    }
}
