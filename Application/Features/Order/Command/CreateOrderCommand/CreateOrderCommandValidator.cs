using CSharpFunctionalExtensions;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Command.CreateOrderCommand
{
    public class CreateOrderCommandValidator
    {
        private readonly DatabaseContext _context;

        public CreateOrderCommandValidator(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result> ValidateAsync(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // Validate SubCategoryId
            if (request.SubCategoryId <= 0)
            {
                return Result.Failure("SubCategory ID is required");
            }

            var subCategoryExists = await _context.SubCategories
                .AnyAsync(sc => sc.SubCategoryId == request.SubCategoryId && sc.IsActive, cancellationToken);
            if (!subCategoryExists)
            {
                return Result.Failure("SubCategory not found or inactive");
            }

            // Validate CityId
            if (request.CityId <= 0)
            {
                return Result.Failure("City ID is required");
            }

            var cityExists = await _context.Cities
                .AnyAsync(c => c.CityId == request.CityId, cancellationToken);
            if (!cityExists)
            {
                return Result.Failure("City not found");
            }

            // Validate ReservationDateFrom
            if (request.ReservationDateFrom < DateTime.UtcNow.Date)
            {
                return Result.Failure("Reservation date from must be a future date");
            }

            // Validate ReservationDateTo
            if (request.ReservationDateTo < request.ReservationDateFrom)
            {
                return Result.Failure("Reservation date to must be greater than or equal to reservation date from");
            }

            // Validate VehiclesCount
            if (request.VehiclesCount <= 0)
            {
                return Result.Failure("Vehicles count must be greater than zero");
            }

            // Validate PassportImage
            if (string.IsNullOrWhiteSpace(request.PassportImage))
            {
                return Result.Failure("Passport image is required");
            }

            // Validate HotelAddress
            if (string.IsNullOrWhiteSpace(request.HotelAddress))
            {
                return Result.Failure("Hotel address is required");
            }

            if (request.HotelAddress.Length > 500)
            {
                return Result.Failure("Hotel address must not exceed 500 characters");
            }

            // Validate PaymentMethodId
            if (!Enum.IsDefined(typeof(PaymentMethod), request.PaymentMethodId))
            {
                return Result.Failure("Invalid payment method");
            }

            // Validate MobileTotal
            if (request.MobileTotal < 0)
            {
                return Result.Failure("Mobile total must be greater than or equal to zero");
            }

            return Result.Success();
        }
    }
}

