using CSharpFunctionalExtensions;
using Domain.Models;
using Infrastructure;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Command.RegisterCustomerCommand
{
    public record RegisterCustomerCommand : IRequest<Result<RegisterCustomerResponse>>
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int CityId { get; set; }
        public string? FullAddress { get; set; }
        public string? PersonalImage { get; set; }
        public int RegisterAs { get; set; } // 0 = Individual, 1 = Institution
        public int VerificationBy { get; set; } // 0 = Phone, 1 = Email
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, Result<RegisterCustomerResponse>>
    {
        private readonly DatabaseContext _context;
        private readonly IInvitationCodeService _invitationCodeService;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly RegisterCustomerCommandValidator _validator;

        public RegisterCustomerCommandHandler(
            DatabaseContext context,
            IInvitationCodeService invitationCodeService,
            IPasswordHasher<ApplicationUser> passwordHasher,
            RegisterCustomerCommandValidator validator)
        {
            _context = context;
            _invitationCodeService = invitationCodeService;
            _passwordHasher = passwordHasher;
            _validator = validator;
        }

        public async Task<Result<RegisterCustomerResponse>> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
        {
            // Validate command using validator
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<RegisterCustomerResponse>(validationResult.Error);
            }

            // Hash password
            var tempUser = new ApplicationUser(); // Just for password hashing
            var passwordHash = _passwordHasher.HashPassword(tempUser, request.Password);

            // Generate invitation code
            var invitationCode = _invitationCodeService.GenerateInvitationCode();

            // Create Customer
            var customer = Domain.Models.Customer.Create(
                request.MobileNumber,
                request.UserName,
                request.FullName,
                request.Gender,
                invitationCode,
                passwordHash,
                request.CityId,
                request.RegisterAs,
                request.VerificationBy,
                request.FullAddress,
                request.PersonalImage,
                "System"
            );

            _context.Customers.Add(customer);

            var saveResult = await _context.SaveChangesAsyncWithResult(cancellationToken);
            if (!saveResult.IsSuccess)
            {
                return Result.Failure<RegisterCustomerResponse>($"Failed to save customer: {saveResult.ErrorMessage}");
            }

            // Send invitation code via SMS (implement SMS service)
            // For now, we'll return it in the response (remove in production)
            await _invitationCodeService.SendInvitationCodeAsync(request.MobileNumber, invitationCode);

            return Result.Success(new RegisterCustomerResponse
            {
                CustomerId = customer.CustomerId,
                InvitationCode = invitationCode, // Remove in production - only for testing
                Message = "Registration successful. Please check your phone for the activation code."
            });
        }
    }

    public class RegisterCustomerResponse
    {
        public int CustomerId { get; set; }
        public string InvitationCode { get; set; } = string.Empty; // Remove in production
        public string Message { get; set; } = string.Empty;
    }
}

