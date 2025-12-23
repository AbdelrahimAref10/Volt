using CSharpFunctionalExtensions;
using Domain.Models;
using Infrastructure;
using Infrastructure.Services;
using Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Admin.Command.AdminLoginCommand
{
    public record AdminLoginCommand : IRequest<Result<AdminLoginResponse>>
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AdminLoginCommandHandler : IRequestHandler<AdminLoginCommand, Result<AdminLoginResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IJwtSettings _jwtSettings;
        private readonly DatabaseContext _context;
        private readonly AdminLoginCommandValidator _validator;

        public AdminLoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService jwtTokenService,
            IJwtSettings jwtSettings,
            DatabaseContext context,
            AdminLoginCommandValidator validator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings;
            _context = context;
            _validator = validator;
        }

        public async Task<Result<AdminLoginResponse>> Handle(AdminLoginCommand request, CancellationToken cancellationToken)
        {
            // Validate command using validator
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<AdminLoginResponse>(validationResult.Error);
            }

            // Find user by username
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
            {
                return Result.Failure<AdminLoginResponse>("Invalid user name or password");
            }

            // Check if user is in Admin role
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin)
            {
                return Result.Failure<AdminLoginResponse>("User is not an administrator");
            }

            // Verify password
            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!signInResult.Succeeded)
            {
                return Result.Failure<AdminLoginResponse>("Invalid user name or password");
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

            return Result.Success(new AdminLoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Roles = roles.ToList()
            });
        }
    }

    public class AdminLoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
}

