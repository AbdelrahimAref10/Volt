using Application.Features.Order.DTOs;
using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Query.Reports.OrdersByDateRangeReportQuery
{
    public record OrdersByDateRangeReportQuery : IRequest<Result<List<OrderDto>>>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class OrdersByDateRangeReportQueryHandler : IRequestHandler<OrdersByDateRangeReportQuery, Result<List<OrderDto>>>
    {
        private readonly DatabaseContext _context;

        public OrdersByDateRangeReportQueryHandler(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Result<List<OrderDto>>> Handle(OrdersByDateRangeReportQuery request, CancellationToken cancellationToken)
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.SubCategory)
                .Include(o => o.City)
                .Where(o => o.CreatedDate >= request.FromDate && o.CreatedDate <= request.ToDate)
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

