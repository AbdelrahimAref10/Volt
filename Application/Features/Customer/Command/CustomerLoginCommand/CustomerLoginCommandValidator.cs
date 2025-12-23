using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Command.CustomerLoginCommand
{
    public class CustomerLoginCommandValidator
    {
        public Task<Result> ValidateAsync(CustomerLoginCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.MobileNumber))
            {
                return Task.FromResult(Result.Failure("Mobile number is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Task.FromResult(Result.Failure("Password is required"));
            }

            return Task.FromResult(Result.Success());
        }
    }
}

