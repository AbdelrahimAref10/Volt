using CSharpFunctionalExtensions;
using Domain.Models;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Command.ActivateCustomerCommand
{
    public record ActivateCustomerCommand : IRequest<Result<bool>>
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string InvitationCode { get; set; } = string.Empty;
    }

    public class ActivateCustomerCommandHandler : IRequestHandler<ActivateCustomerCommand, Result<bool>>
    {
        private readonly DatabaseContext _context;
        private readonly ActivateCustomerCommandValidator _validator;

        public ActivateCustomerCommandHandler(
            DatabaseContext context,
            ActivateCustomerCommandValidator validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result<bool>> Handle(ActivateCustomerCommand request, CancellationToken cancellationToken)
        {
            // Validate command using validator
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<bool>(validationResult.Error);
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.MobileNumber == request.MobileNumber, cancellationToken);

            // Customer should exist (validator already checked), but add null check for safety
            if (customer == null)
            {
                return Result.Failure<bool>("Customer not found");
            }

            // Activate customer
            customer.Activate("System");

            var saveResult = await _context.SaveChangesAsyncWithResult(cancellationToken);
            if (!saveResult.IsSuccess)
            {
                return Result.Failure<bool>($"Failed to activate customer: {saveResult.ErrorMessage}");
            }

            return Result.Success(true);
        }
    }
}

