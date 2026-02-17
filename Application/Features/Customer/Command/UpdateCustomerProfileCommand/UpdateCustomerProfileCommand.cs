using CSharpFunctionalExtensions;
using Domain.Common;
using Domain.Models;
using Infrastructure;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Command.UpdateCustomerProfileCommand
{
    public record UpdateCustomerProfileCommand : IRequest<Result<bool>>
    {
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int CityId { get; set; }
        public string? Email { get; set; }
        public string? PersonalImage { get; set; }
        public string? CommercialRegisterImage { get; set; }
        public string? Password { get; set; } // Optional - only update if provided
    }

    public class UpdateCustomerProfileCommandHandler : IRequestHandler<UpdateCustomerProfileCommand, Result<bool>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;
        private readonly IImageService _imageService;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly IDateTimeProvider _dateTimeProvider;

        public UpdateCustomerProfileCommandHandler(
            DatabaseContext context,
            IUserSession userSession,
            IImageService imageService,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _userSession = userSession;
            _imageService = imageService;
            _passwordHasher = passwordHasher;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<bool>> Handle(UpdateCustomerProfileCommand request, CancellationToken cancellationToken)
        {
            // Get customer ID from session
            var customerId = _userSession.UserId;
            if (customerId <= 0)
            {
                return Result.Failure<bool>("Customer not authenticated");
            }

            // Validate command using validator
            var validator = new UpdateCustomerProfileCommandValidator(_context);
            var validationResult = await validator.ValidateAsync(request, customerId, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<bool>(validationResult.Error);
            }

            var customer = await _context.Customers
                .AsTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

            if (customer == null)
            {
                return Result.Failure<bool>("Customer not found");
            }

            // Save base64 images as files and get URLs
            // Store old image URLs for deletion if new images are provided
            string? oldPersonalImage = customer.PersonalImage;
            string? oldCommercialRegisterImage = customer.CommercialRegisterImage;

            string? personalImageUrl = customer.PersonalImage; // Keep existing if no new image provided
            if (!string.IsNullOrWhiteSpace(request.PersonalImage) && _imageService.IsBase64String(request.PersonalImage))
            {
                personalImageUrl = _imageService.SaveBase64Image(request.PersonalImage, "customers");
                // Delete old image if it exists and is different
                if (!string.IsNullOrWhiteSpace(oldPersonalImage) && oldPersonalImage != personalImageUrl)
                {
                    _imageService.DeleteImage(oldPersonalImage);
                }
            }

            string? commercialRegisterImageUrl = customer.CommercialRegisterImage; // Keep existing if no new image provided
            if (!string.IsNullOrWhiteSpace(request.CommercialRegisterImage) && _imageService.IsBase64String(request.CommercialRegisterImage))
            {
                commercialRegisterImageUrl = _imageService.SaveBase64Image(request.CommercialRegisterImage, "customers");
                // Delete old image if it exists and is different
                if (!string.IsNullOrWhiteSpace(oldCommercialRegisterImage) && oldCommercialRegisterImage != commercialRegisterImageUrl)
                {
                    _imageService.DeleteImage(oldCommercialRegisterImage);
                }
            }

            // Update profile using domain method
            customer.UpdateProfile(
                request.FullName,
                request.Gender,
                request.CityId,
                request.Email,
                personalImageUrl,
                commercialRegisterImageUrl,
                customerId.ToString()
            );

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var tempUser = new ApplicationUser(); // Just for password hashing
                var passwordHash = _passwordHasher.HashPassword(tempUser, request.Password);
                customer.ResetPassword(passwordHash);
            }

            var saveResult = await _context.SaveChangesAsyncWithResult(cancellationToken);
            if (!saveResult.IsSuccess)
            {
                return Result.Failure<bool>($"Failed to update customer profile: {saveResult.ErrorMessage}");
            }

            return Result.Success(true);
        }
    }
}

