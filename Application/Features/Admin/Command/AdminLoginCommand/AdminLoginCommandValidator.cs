using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Admin.Command.AdminLoginCommand
{
    public class AdminLoginCommandValidator
    {
        public Task<Result> ValidateAsync(AdminLoginCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                return Task.FromResult(Result.Failure("User name is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Task.FromResult(Result.Failure("Password is required"));
            }

            return Task.FromResult(Result.Success());
        }
    }
}

