using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.Models;
using Infrastructure;
using Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Command.ResendActivationCodeCommand
{
    public record ResendActivationCodeCommand : IRequest<Result<ResendActivationCodeResponse>>
    {
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
    }

    public class ResendActivationCodeCommandHandler : IRequestHandler<ResendActivationCodeCommand, Result<ResendActivationCodeResponse>>
    {
        private readonly DatabaseContext _context;
        private readonly IInvitationCodeService _invitationCodeService;
        private readonly ResendActivationCodeCommandValidator _validator;

        public ResendActivationCodeCommandHandler(
            DatabaseContext context,
            IInvitationCodeService invitationCodeService,
            ResendActivationCodeCommandValidator validator)
        {
            _context = context;
            _invitationCodeService = invitationCodeService;
            _validator = validator;
        }

        public async Task<Result<ResendActivationCodeResponse>> Handle(ResendActivationCodeCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<ResendActivationCodeResponse>(validationResult.Error);
            }

            var customer = await GetCustomerAsync(request, cancellationToken);
            if (customer == null)
            {
                return Result.Failure<ResendActivationCodeResponse>("Customer not found");
            }

            if (customer.State != CustomerState.InActive)
            {
                return Result.Failure<ResendActivationCodeResponse>("Customer is already activated");
            }

            if (string.IsNullOrWhiteSpace(customer.InvitationCode))
            {
                return Result.Failure<ResendActivationCodeResponse>("No activation code found. Please register again.");
            }

            if (customer.InvitationCodeExpiry.HasValue && customer.InvitationCodeExpiry.Value < System.DateTime.UtcNow)
            {
                return Result.Failure<ResendActivationCodeResponse>("Activation code has expired. Please register again.");
            }

            // Resend the existing code (no new code, no DB update)
            var code = customer.InvitationCode;

            // Decide where to send: only phone → SMS; only email → Email; both → use VerificationBy
            var sendToEmail = ShouldSendToEmail(request, customer);
            if (sendToEmail && !string.IsNullOrWhiteSpace(customer.Email))
            {
                await _invitationCodeService.SendInvitationCodeAsync(
                    customer.MobileNumber,
                    customer.Email,
                    (int)VerificationBy.Email,
                    code);
                return Result.Success(new ResendActivationCodeResponse
                {
                    Message = "Activation code has been sent to your email."
                });
            }

            await _invitationCodeService.SendInvitationCodeAsync(
                customer.MobileNumber,
                customer.Email,
                (int)VerificationBy.Phone,
                code);
            return Result.Success(new ResendActivationCodeResponse
            {
                Message = "Activation code has been sent to your phone."
            });
        }

        private static bool ShouldSendToEmail(ResendActivationCodeCommand request, Domain.Models.Customer customer)
        {
            var hasPhone = !string.IsNullOrWhiteSpace(request.MobileNumber);
            var hasEmail = !string.IsNullOrWhiteSpace(request.Email);

            if (hasPhone && !hasEmail)
                return false; // Only phone provided → send SMS
            if (!hasPhone && hasEmail)
                return true;  // Only email provided → send Email
            // Both provided → use customer's VerificationBy
            return customer.VerificationBy == (int)VerificationBy.Email;
        }

        private async Task<Domain.Models.Customer> GetCustomerAsync(ResendActivationCodeCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(request.MobileNumber))
            {
                return await _context.Customers
                    .FirstOrDefaultAsync(c => c.MobileNumber == request.MobileNumber, cancellationToken);
            }
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                return await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == request.Email, cancellationToken);
            }
            return null;
        }
    }

    public class ResendActivationCodeResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
