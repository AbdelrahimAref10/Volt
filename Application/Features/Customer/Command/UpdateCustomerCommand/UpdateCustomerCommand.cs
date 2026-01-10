using CSharpFunctionalExtensions;
using Domain.Common;
using Domain.Models;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Command.UpdateCustomerCommand
{
    public record UpdateCustomerCommand : IRequest<Result<bool>>
    {
        public int CustomerId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int CityId { get; set; }
        public string? FullAddress { get; set; }
        public string? PersonalImage { get; set; }
    }

    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<bool>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;
        private readonly UpdateCustomerCommandValidator _validator;

        public UpdateCustomerCommandHandler(
            DatabaseContext context, 
            IUserSession userSession,
            UpdateCustomerCommandValidator validator)
        {
            _context = context;
            _userSession = userSession;
            _validator = validator;
        }

        public async Task<Result<bool>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            // Validate command using validator
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<bool>(validationResult.Error);
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

            customer.UpdateProfile(
                request.UserName,
                request.FullName,
                request.Gender,
                request.CityId,
                request.FullAddress,
                request.PersonalImage,
                _userSession.UserId.ToString()
            );

            var saveResult = await _context.SaveChangesAsyncWithResult(cancellationToken);
            if (!saveResult.IsSuccess)
            {
                return Result.Failure<bool>($"Failed to update customer: {saveResult.ErrorMessage}");
            }

            return Result.Success(true);
        }
    }
}

