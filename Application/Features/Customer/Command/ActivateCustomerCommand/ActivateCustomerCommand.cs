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

        public ActivateCustomerCommandHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(ActivateCustomerCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.MobileNumber))
            {
                return Result.Failure<bool>("Mobile number is required");
            }

            if (string.IsNullOrWhiteSpace(request.InvitationCode))
            {
                return Result.Failure<bool>("Invitation code is required");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.MobileNumber == request.MobileNumber, cancellationToken);

            if (customer == null)
            {
                return Result.Failure<bool>("Customer not found");
            }

            if (customer.State == Domain.Enums.CustomerState.Active)
            {
                return Result.Failure<bool>("Customer is already activated");
            }

            // Validate invitation code
            if (!customer.ValidateInvitationCode(request.InvitationCode))
            {
                return Result.Failure<bool>("Invalid or expired invitation code");
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

