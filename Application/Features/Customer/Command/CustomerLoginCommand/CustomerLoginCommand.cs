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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly DatabaseContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IJwtSettings _jwtSettings;

        public CustomerLoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            DatabaseContext context,
            IJwtTokenService jwtTokenService,
            IJwtSettings jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings;
        }

        public async Task<Result<LoginResponse>> Handle(CustomerLoginCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.MobileNumber))
            {
                return Result.Failure<LoginResponse>("Mobile number is required");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Result.Failure<LoginResponse>("Password is required");
            }

            // Find user by mobile number (username)
            var user = await _userManager.FindByNameAsync(request.MobileNumber);
            if (user == null)
            {
                return Result.Failure<LoginResponse>("Invalid mobile number or password");
            }

            // Check if user is in Customer role
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            if (!isCustomer)
            {
                return Result.Failure<LoginResponse>("User is not a customer");
            }

            // Find customer record
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id, cancellationToken);

            if (customer == null)
            {
                return Result.Failure<LoginResponse>("Customer record not found");
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
            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!signInResult.Succeeded)
            {
                return Result.Failure<LoginResponse>("Invalid mobile number or password");
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user, roles);

            // Generate refresh token
            var refreshToken = _jwtTokenService.GenerateRefreshToken();
            var refreshTokenExpires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

            // Save refresh token to database
            var refreshTokenEntity = Domain.Models.RefreshToken.Create(
                user.Id,
                refreshToken,
                refreshTokenExpires,
                user.UserName ?? "System"
            );

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Roles = roles.ToList(),
                CustomerId = customer.CustomerId
            });
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

