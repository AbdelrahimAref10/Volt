using CSharpFunctionalExtensions;
using Domain.Enums;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Command.AdminCreateCustomerCommand
{
    public class AdminCreateCustomerCommandValidator
    {
        private readonly DatabaseContext _context;

        public AdminCreateCustomerCommandValidator(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result> ValidateAsync(AdminCreateCustomerCommand request, CancellationToken cancellationToken)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.MobileNumber))
            {
                return Result.Failure("Mobile number is required");
            }

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return Result.Failure("Full name is required");
            }

            if (string.IsNullOrWhiteSpace(request.Gender))
            {
                return Result.Failure("Gender is required");
            }

            // Validate RegisterAs enum value
            if (!Enum.IsDefined(typeof(RegisterAs), request.RegisterAs))
            {
                return Result.Failure("Invalid RegisterAs value");
            }

            // Validate VerificationBy enum value
            if (!Enum.IsDefined(typeof(VerificationBy), request.VerificationBy))
            {
                return Result.Failure("Invalid VerificationBy value");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Result.Failure("Password is required");
            }

            if (request.Password.Length < 6)
            {
                return Result.Failure("Password must be at least 6 characters long");
            }

            if (request.CityId <= 0)
            {
                return Result.Failure("Valid city is required");
            }

            // Validate that City exists and is active
            var cityExists = await _context.Cities.AnyAsync(c => c.CityId == request.CityId && c.IsActive, cancellationToken);
            if (!cityExists)
            {
                return Result.Failure("Invalid or inactive city");
            }

            // Check if customer with this mobile number already exists
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.MobileNumber == request.MobileNumber, cancellationToken);

            if (existingCustomer != null)
            {
                return Result.Failure("Customer with this mobile number already exists");
            }

            return Result.Success();
        }
    }
}

