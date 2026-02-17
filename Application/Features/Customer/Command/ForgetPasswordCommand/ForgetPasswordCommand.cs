using CSharpFunctionalExtensions;
using Domain.Common;
using Domain.Enums;
using Domain.Models;
using Infrastructure;
using Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Command.ForgetPasswordCommand
{
    public record ForgetPasswordCommand : IRequest<Result<ForgetPasswordResponse>>
    {
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
    }

    public class ForgetPasswordCommandHandler : IRequestHandler<ForgetPasswordCommand, Result<ForgetPasswordResponse>>
    {
        private readonly DatabaseContext _context;
        private readonly IInvitationCodeService _invitationCodeService;
        private readonly ForgetPasswordCommandValidator _validator;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ForgetPasswordCommandHandler(
            DatabaseContext context,
            IInvitationCodeService invitationCodeService,
            ForgetPasswordCommandValidator validator,
            IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _invitationCodeService = invitationCodeService;
            _validator = validator;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<ForgetPasswordResponse>> Handle(ForgetPasswordCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<ForgetPasswordResponse>(validationResult.Error);
            }

            var customer = await GetCustomerAsync(request, cancellationToken);
            if (customer == null)
            {
                return Result.Failure<ForgetPasswordResponse>("Customer not found");
            }

            if (customer.State != CustomerState.Active)
            {
                return Result.Failure<ForgetPasswordResponse>("Account is not active. Please activate your account first.");
            }

            var code = _invitationCodeService.GenerateInvitationCode();
            customer.SetPasswordResetCode(code, _dateTimeProvider, expiryMinutes: 15);

            var saveResult = await _context.SaveChangesAsyncWithResult(cancellationToken);
            if (!saveResult.IsSuccess)
            {
                return Result.Failure<ForgetPasswordResponse>($"Failed to update: {saveResult.ErrorMessage}");
            }

            var sendToEmail = ShouldSendToEmail(request, customer);
            if (sendToEmail && !string.IsNullOrWhiteSpace(customer.Email))
            {
                await _invitationCodeService.SendInvitationCodeAsync(
                    customer.MobileNumber,
                    customer.Email,
                    (int)VerificationBy.Email,
                    code);
                return Result.Success(new ForgetPasswordResponse
                {
                    Message = "Password reset code has been sent to your email."
                });
            }

            await _invitationCodeService.SendInvitationCodeAsync(
                customer.MobileNumber,
                customer.Email,
                (int)VerificationBy.Phone,
                code);
            return Result.Success(new ForgetPasswordResponse
            {
                Message = "Password reset code has been sent to your phone."
            });
        }

        private static bool ShouldSendToEmail(ForgetPasswordCommand request, Domain.Models.Customer customer)
        {
            var hasPhone = !string.IsNullOrWhiteSpace(request.MobileNumber);
            var hasEmail = !string.IsNullOrWhiteSpace(request.Email);

            if (hasPhone && !hasEmail)
                return false;
            if (!hasPhone && hasEmail)
                return true;
            return customer.VerificationBy == (int)VerificationBy.Email;
        }

        private async Task<Domain.Models.Customer?> GetCustomerAsync(ForgetPasswordCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(request.MobileNumber))
            {
                return await _context.Customers.AsTracking()
                    .FirstOrDefaultAsync(c => c.MobileNumber == request.MobileNumber, cancellationToken);
            }
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                return await _context.Customers.AsTracking()
                    .FirstOrDefaultAsync(c => c.Email == request.Email, cancellationToken);
            }
            return null;
        }
    }

    public class ForgetPasswordResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
