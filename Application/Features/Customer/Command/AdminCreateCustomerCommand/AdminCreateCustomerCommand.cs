using CSharpFunctionalExtensions;
using Domain.Common;
using Domain.Models;
using Infrastructure;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Command.AdminCreateCustomerCommand
{
    public record AdminCreateCustomerCommand : IRequest<Result<int>>
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int CityId { get; set; }
        public string? PersonalImage { get; set; }
        public string? Email { get; set; } // Required if VerificationBy = 1 (Email)
        public string? CommercialRegisterImage { get; set; }
        public int RegisterAs { get; set; } // 0 = Individual, 1 = Institution
        public int VerificationBy { get; set; } // 0 = Phone, 1 = Email
        public string Password { get; set; } = string.Empty;
    }

    public class AdminCreateCustomerCommandHandler : IRequestHandler<AdminCreateCustomerCommand, Result<int>>
    {
        private readonly DatabaseContext _context;
        private readonly IInvitationCodeService _invitationCodeService;
        private readonly IUserSession _userSession;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly IImageService _imageService;
        private readonly AdminCreateCustomerCommandValidator _validator;

        public AdminCreateCustomerCommandHandler(
            DatabaseContext context,
            IInvitationCodeService invitationCodeService,
            IUserSession userSession,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IImageService imageService,
            AdminCreateCustomerCommandValidator validator)
        {
            _context = context;
            _invitationCodeService = invitationCodeService;
            _userSession = userSession;
            _passwordHasher = passwordHasher;
            _imageService = imageService;
            _validator = validator;
        }

        public async Task<Result<int>> Handle(AdminCreateCustomerCommand request, CancellationToken cancellationToken)
        {
            // Validate command using validator
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<int>(validationResult.Error);
            }

            // Hash password
            var tempUser = new ApplicationUser(); // Just for password hashing
            var passwordHash = _passwordHasher.HashPassword(tempUser, request.Password);

            // Generate invitation code (store in DB for record keeping, but customer will be active immediately)
            var invitationCode = _invitationCodeService.GenerateInvitationCode();

            // Save base64 images as files and get URLs
            string? personalImageUrl = null;
            if (!string.IsNullOrWhiteSpace(request.PersonalImage))
            {
                try
                {
                    personalImageUrl = _imageService.SaveBase64Image(request.PersonalImage, "customers");
                }
                catch (Exception ex)
                {
                    return Result.Failure<int>($"Failed to save personal image: {ex.Message}");
                }
            }

            string? commercialRegisterImageUrl = null;
            if (request.RegisterAs == (int)Domain.Enums.RegisterAs.Institution && !string.IsNullOrWhiteSpace(request.CommercialRegisterImage))
            {
                try
                {
                    commercialRegisterImageUrl = _imageService.SaveBase64Image(request.CommercialRegisterImage, "customers");
                }
                catch (Exception ex)
                {
                    return Result.Failure<int>($"Failed to save commercial register image: {ex.Message}");
                }
            }

            // Create Customer (will be created as InActive initially)
            var customer = Domain.Models.Customer.Create(
                request.MobileNumber,
                request.FullName,
                request.Gender,
                invitationCode,
                passwordHash,
                request.CityId,
                request.RegisterAs,
                request.VerificationBy,
                request.Email,
                personalImageUrl,
                commercialRegisterImageUrl,
                _userSession.UserName ?? "Admin"
            );

            // Activate customer immediately without clearing invitation code
            // This keeps the code in DB for record keeping but makes customer active
            customer.ActivateWithoutClearingCode(_userSession.UserName ?? "Admin");

            _context.Customers.Add(customer);

            // Save customer (already activated, code stored in DB)
            var saveResult = await _context.SaveChangesAsyncWithResult(cancellationToken);
            if (!saveResult.IsSuccess)
            {
                return Result.Failure<int>($"Failed to save customer: {saveResult.ErrorMessage}");
            }

            // Note: No SMS sent for admin-created customers - they are already active

            return Result.Success(customer.CustomerId);
        }
    }
}

