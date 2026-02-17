using CSharpFunctionalExtensions;
using Domain.Common;
using Domain.Models;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Command.ResetPasswordCommand
{
    public record ResetPasswordCommand : IRequest<Result<ResetPasswordResponse>>
    {
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public string ResetCode { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<ResetPasswordResponse>>
    {
        private readonly DatabaseContext _context;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly ResetPasswordCommandValidator _validator;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ResetPasswordCommandHandler(
            DatabaseContext context,
            IPasswordHasher<ApplicationUser> passwordHasher,
            ResetPasswordCommandValidator validator,
            IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _validator = validator;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<ResetPasswordResponse>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<ResetPasswordResponse>(validationResult.Error);
            }

            var customer = await GetCustomerAsync(request, cancellationToken);
            if (customer == null)
            {
                return Result.Failure<ResetPasswordResponse>("Customer not found");
            }

            if (!customer.ValidatePasswordResetCode(request.ResetCode, _dateTimeProvider))
            {
                return Result.Failure<ResetPasswordResponse>("Invalid or expired reset code. Please request a new code.");
            }

            var tempUser = new ApplicationUser();
            var passwordHash = _passwordHasher.HashPassword(tempUser, request.NewPassword);
            customer.ResetPassword(passwordHash);

            var saveResult = await _context.SaveChangesAsyncWithResult(cancellationToken);
            if (!saveResult.IsSuccess)
            {
                return Result.Failure<ResetPasswordResponse>($"Failed to reset password: {saveResult.ErrorMessage}");
            }

            return Result.Success(new ResetPasswordResponse
            {
                Message = "Password has been reset successfully. You can now login with your new password."
            });
        }

        private async Task<Domain.Models.Customer?> GetCustomerAsync(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(request.MobileNumber))
            {
                return await _context.Customers
                    .AsTracking()
                    .FirstOrDefaultAsync(c => c.MobileNumber == request.MobileNumber, cancellationToken);
            }
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                return await _context.Customers
                    .AsTracking()
                    .FirstOrDefaultAsync(c => c.Email == request.Email, cancellationToken);
            }
            return null;
        }
    }

    public class ResetPasswordResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
