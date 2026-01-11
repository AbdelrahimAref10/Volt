using Application.Features.Order.DTOs;
using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Query.Reports.CustomerOrderHistoryReportQuery
{
    public record CustomerOrderHistoryReportQuery : IRequest<Result<List<OrderDto>>>
    {
        public int CustomerId { get; set; }
    }

    public class CustomerOrderHistoryReportQueryHandler : IRequestHandler<CustomerOrderHistoryReportQuery, Result<List<OrderDto>>>
    {
        private readonly DatabaseContext _context;

        public CustomerOrderHistoryReportQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<List<OrderDto>>> Handle(CustomerOrderHistoryReportQuery request, CancellationToken cancellationToken)
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.SubCategory)
                .Include(o => o.City)
                .Where(o => o.CustomerId == request.CustomerId)
                .OrderByDescending(o => o.CreatedDate)
                .Select(o => new OrderDto
                {
                    OrderId = o.OrderId,
                    OrderCode = o.OrderCode,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.FullName,
                    SubCategoryId = o.SubCategoryId,
                    SubCategoryName = o.SubCategory.Name,
                    CityId = o.CityId,
                    CityName = o.City.Name,
                    ReservationDateFrom = o.ReservationDateFrom,
                    ReservationDateTo = o.ReservationDateTo,
                    VehiclesCount = o.VehiclesCount,
                    OrderSubTotal = o.OrderSubTotal,
                    OrderTotal = o.OrderTotal,
                    Notes = o.Notes,
                    HotelName = o.HotelName,
                    HotelAddress = o.HotelAddress,
                    HotelPhone = o.HotelPhone,
                    IsUrgent = o.IsUrgent,
                    PaymentMethod = (Domain.Enums.PaymentMethod)o.PaymentMethodId,
                    OrderState = o.OrderState,
                    CreatedDate = o.CreatedDate
                })
                .ToListAsync(cancellationToken);

            return Result.Success(orders);
        }
    }
}

