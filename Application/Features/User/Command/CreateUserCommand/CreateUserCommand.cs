using Application.Features.User.DTOs;
using CSharpFunctionalExtensions;
using Domain.Common;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.User.Command.CreateUserCommand
{
    public record CreateUserCommand : IRequest<Result<int>>
    {
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<int>>
    {
        private readonly UserManager<Domain.Models.ApplicationUser> _userManager;
        private readonly RoleManager<Domain.Models.ApplicationRole> _roleManager;
        private readonly IUserSession _userSession;

        public CreateUserCommandHandler(
            UserManager<Domain.Models.ApplicationUser> userManager,
            RoleManager<Domain.Models.ApplicationRole> roleManager,
            IUserSession userSession)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userSession = userSession;
        }

        public async Task<Result<int>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return Result.Failure<int>("Email is required");
            }

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return Result.Failure<int>("Phone number is required");
            }

            // Validate that the role exists
            var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());
            if (role == null)
            {
                return Result.Failure<int>($"Role with ID '{request.RoleId}' does not exist");
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByNameAsync(request.UserName);
            if (existingUser != null)
            {
                return Result.Failure<int>("User with this username already exists");
            }

            existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Result.Failure<int>("User with this email already exists");
            }

            // Create user
            var user = new Domain.Models.ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = false,
                PhoneNumberConfirmed = false,
                Active = true,
                CreatedBy = _userSession.UserName,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure<int>($"Failed to create user: {errors}");
            }

            // Assign role
            var roleResult = await _userManager.AddToRoleAsync(user, role.Name!);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return Result.Failure<int>($"Failed to assign role: {errors}");
            }

            return Result.Success(user.Id);
        }
    }
}


