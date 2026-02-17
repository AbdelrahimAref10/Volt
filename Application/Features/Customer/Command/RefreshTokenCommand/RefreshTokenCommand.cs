using CSharpFunctionalExtensions;
using Domain.Common;
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

namespace Application.Features.Customer.Command.RefreshTokenCommand
{
    public record RefreshTokenCommand : IRequest<Result<RefreshTokenResponse>>
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IJwtSettings _jwtSettings;
        private readonly IDateTimeProvider _dateTimeProvider;

        public RefreshTokenCommandHandler(
            DatabaseContext context,
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService,
            IJwtSettings jwtSettings,
            IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return Result.Failure<RefreshTokenResponse>("Refresh token is required");
            }

            // Find refresh token in database
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

            if (refreshToken == null)
            {
                return Result.Failure<RefreshTokenResponse>("Invalid refresh token");
            }

            // Check if token is active
            if (!refreshToken.IsActive(_dateTimeProvider))
            {
                return Result.Failure<RefreshTokenResponse>("Refresh token has been revoked or expired");
            }

            // Revoke old refresh token
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
            refreshToken.Revoke(_dateTimeProvider, newRefreshToken);

            // Get user roles
            var roles = await _userManager.GetRolesAsync(refreshToken.User);

            // Generate new JWT token
            var newToken = _jwtTokenService.GenerateToken(refreshToken.User, roles);

            // Create new refresh token
            var newRefreshTokenEntity = Domain.Models.RefreshToken.Create(
                refreshToken.UserId,
                newRefreshToken,
                _dateTimeProvider.Now.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                refreshToken.User.UserName ?? "System",
                _dateTimeProvider
            );

            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(new RefreshTokenResponse
            {
                Token = newToken,
                RefreshToken = newRefreshToken
            });
        }
    }

    public class RefreshTokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}

