using Application.Features.Customer.DTOs;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.Models;
using Infrastructure;
using Infrastructure.Services;
using Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Command.CustomerLoginCommand
{
    public record CustomerLoginCommand : IRequest<Result<LoginResponse>>
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CustomerLoginCommandHandler : IRequestHandler<CustomerLoginCommand, Result<LoginResponse>>
    {
        private readonly DatabaseContext _context;
        private readonly IJwtSettings _jwtSettings;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly CustomerLoginCommandValidator _validator;

        public CustomerLoginCommandHandler(
            DatabaseContext context,
            IJwtSettings jwtSettings,
            IPasswordHasher<ApplicationUser> passwordHasher,
            CustomerLoginCommandValidator validator)
        {
            _context = context;
            _jwtSettings = jwtSettings;
            _passwordHasher = passwordHasher;
            _validator = validator;
        }

        public async Task<Result<LoginResponse>> Handle(CustomerLoginCommand request, CancellationToken cancellationToken)
        {
            // Validate command using validator
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<LoginResponse>(validationResult.Error);
            }

            // Find customer by mobile number
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.MobileNumber == request.MobileNumber, cancellationToken);

            if (customer == null)
            {
                return Result.Failure<LoginResponse>("Invalid mobile number or password");
            }

            // Check if customer is activated and not blocked
            if (customer.State != CustomerState.Active)
            {
                if (customer.State == CustomerState.Blocked)
                {
                    return Result.Failure<LoginResponse>("Customer account is blocked. Please contact support.");
                }
                return Result.Failure<LoginResponse>("Customer account is not activated. Please activate your account first.");
            }

            // Verify password
            var tempUser = new ApplicationUser(); // Just for password verification
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(tempUser, customer.PasswordHash, request.Password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                return Result.Failure<LoginResponse>("Invalid mobile number or password");
            }

            // Generate JWT token for customer
            var token = GenerateCustomerToken(customer);

            // Generate refresh token
            var refreshToken = GenerateRefreshToken();
            
            // Note: RefreshToken is currently linked to ApplicationUser
            // For customers, we'll return the refresh token but not store it in RefreshTokens table
            // This can be updated later if needed

            return Result.Success(new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                UserId = customer.CustomerId, // Return CustomerId as UserId for compatibility
                UserName = customer.MobileNumber, // Use MobileNumber instead of UserName
                Roles = new List<string> { "Customer" },
                CustomerId = customer.CustomerId
            });
        }

        private string GenerateCustomerToken(Domain.Models.Customer customer)
        {
            if (string.IsNullOrWhiteSpace(_jwtSettings.Key))
            {
                throw new InvalidOperationException("JWT Key is not configured");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
                new Claim(ClaimTypes.Name, customer.MobileNumber), // Use MobileNumber instead of UserName
                new Claim("MobileNumber", customer.MobileNumber),
                new Claim(ClaimTypes.Role, "Customer"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public int CustomerId { get; set; }
    }
}

