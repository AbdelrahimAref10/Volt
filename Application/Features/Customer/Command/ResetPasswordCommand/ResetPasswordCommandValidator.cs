using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Command.ResetPasswordCommand
{
    public class ResetPasswordCommandValidator
    {
        private readonly DatabaseContext _context;

        public ResetPasswordCommandValidator(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result> ValidateAsync(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var hasPhone = !string.IsNullOrWhiteSpace(request.MobileNumber);
            var hasEmail = !string.IsNullOrWhiteSpace(request.Email);

            if (!hasPhone && !hasEmail)
            {
                return Result.Failure("Mobile number or email is required");
            }

            if (string.IsNullOrWhiteSpace(request.ResetCode))
            {
                return Result.Failure("Reset code is required");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return Result.Failure("New password is required");
            }

            if (request.NewPassword.Length < 6)
            {
                return Result.Failure("Password must be at least 6 characters long");
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

            return Result.Success();
        }
    }
}
