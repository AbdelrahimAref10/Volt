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
        public string NationalNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
    }

    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<bool>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;

        public UpdateCustomerCommandHandler(DatabaseContext context, IUserSession userSession)
        {
            _context = context;
            _userSession = userSession;
        }

        public async Task<Result<bool>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

            if (customer == null)
            {
                return Result.Failure<bool>("Customer not found");
            }

            customer.UpdateProfile(
                request.UserName,
                request.NationalNumber,
                request.Gender,
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

