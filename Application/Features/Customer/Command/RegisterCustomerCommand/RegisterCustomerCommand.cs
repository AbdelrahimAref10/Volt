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
        public string NationalNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, Result<RegisterCustomerResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DatabaseContext _context;
        private readonly IInvitationCodeService _invitationCodeService;

        public RegisterCustomerCommandHandler(
            UserManager<ApplicationUser> userManager,
            DatabaseContext context,
            IInvitationCodeService invitationCodeService)
        {
            _userManager = userManager;
            _context = context;
            _invitationCodeService = invitationCodeService;
        }

        public async Task<Result<RegisterCustomerResponse>> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
        {
            // Validate mobile number format
            if (string.IsNullOrWhiteSpace(request.MobileNumber))
            {
                return Result.Failure<RegisterCustomerResponse>("Mobile number is required");
            }

            // Check if customer with this mobile number already exists
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.MobileNumber == request.MobileNumber, cancellationToken);

            if (existingCustomer != null)
            {
                return Result.Failure<RegisterCustomerResponse>("Customer with this mobile number already exists");
            }

            // Check if national number already exists
            var existingNationalNumber = await _context.Customers
                .FirstOrDefaultAsync(c => c.NationalNumber == request.NationalNumber, cancellationToken);

            if (existingNationalNumber != null)
            {
                return Result.Failure<RegisterCustomerResponse>("Customer with this national number already exists");
            }

            // Create ApplicationUser
            var user = new ApplicationUser
            {
                UserName = request.MobileNumber, // Use mobile number as username
                Email = $"{request.MobileNumber}@customer.com", // Temporary email
                PhoneNumber = request.MobileNumber,
                PhoneNumberConfirmed = false,
                EmailConfirmed = false
            };

            var createUserResult = await _userManager.CreateAsync(user, request.Password);
            if (!createUserResult.Succeeded)
            {
                var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                return Result.Failure<RegisterCustomerResponse>($"Failed to create user: {errors}");
            }

            // Assign Customer role
            var addToRoleResult = await _userManager.AddToRoleAsync(user, "Customer");
            if (!addToRoleResult.Succeeded)
            {
                // Rollback user creation
                await _userManager.DeleteAsync(user);
                var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                return Result.Failure<RegisterCustomerResponse>($"Failed to assign role: {errors}");
            }

            // Generate invitation code
            var invitationCode = _invitationCodeService.GenerateInvitationCode();

            // Create Customer
            var customer = Domain.Models.Customer.Create(
                request.MobileNumber,
                request.UserName,
                request.NationalNumber,
                request.Gender,
                invitationCode,
                user.Id,
                "System"
            );

            _context.Customers.Add(customer);

            var saveResult = await _context.SaveChangesAsyncWithResult(cancellationToken);
            if (!saveResult.IsSuccess)
            {
                await _userManager.DeleteAsync(user);
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

