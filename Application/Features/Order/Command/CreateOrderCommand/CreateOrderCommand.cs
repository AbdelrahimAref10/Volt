using Application.Features.Order.DTOs;
using CSharpFunctionalExtensions;
using Domain.Common;
using Domain.Enums;
using Domain.Models;
using Domain.Services;
using Infrastructure;
using Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Order.Command.CreateOrderCommand
{
    public record CreateOrderCommand : IRequest<Result<OrderDto>>
    {
        public int SubCategoryId { get; set; }
        public int CityId { get; set; }
        public DateTime ReservationDateFrom { get; set; }
        public DateTime ReservationDateTo { get; set; }
        public int VehiclesCount { get; set; }
        public string? Notes { get; set; }
        public string PassportImage { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
        public string HotelAddress { get; set; } = string.Empty;
        public string? HotelPhone { get; set; }
        public bool IsUrgent { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal MobileTotal { get; set; }
    }

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
    {
        private readonly DatabaseContext _context;
        private readonly IUserSession _userSession;
        private readonly IPayPalService _payPalService;

        public CreateOrderCommandHandler(DatabaseContext context, IUserSession userSession, IPayPalService payPalService)
        {
            _context = context;
            _userSession = userSession;
            _payPalService = payPalService;
        }

        public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // Validate command
            var validator = new CreateOrderCommandValidator(_context);
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<OrderDto>(validationResult.Error);
            }

            var customerId = _userSession.UserId;

            if (customerId <= 0)
            {
                return Result.Failure<OrderDto>("Customer not found or not authenticated");
            }

            // Get customer and validate
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

            if (customer == null)
            {
                return Result.Failure<OrderDto>("Customer not found");
            }

            // Validate payment method (check CashBlock for Cash)
            if (request.PaymentMethodId == (int)PaymentMethod.Cash && customer.CashBlock)
            {
                return Result.Failure<OrderDto>("Payment method not allowed. Cash payment is blocked for this customer.");
            }

            // Get subcategory
            var subCategory = await _context.SubCategories
                .FirstOrDefaultAsync(sc => sc.SubCategoryId == request.SubCategoryId && sc.IsActive, cancellationToken);

            if (subCategory == null)
            {
                return Result.Failure<OrderDto>("SubCategory not found or inactive");
            }

            // Get city
            var city = await _context.Cities
                .FirstOrDefaultAsync(c => c.CityId == request.CityId, cancellationToken);

            if (city == null)
            {
                return Result.Failure<OrderDto>("City not found");
            }

            // Validate date range
            if (request.ReservationDateFrom >= request.ReservationDateTo)
            {
                return Result.Failure<OrderDto>("Reservation date from must be before reservation date to");
            }

            if (request.ReservationDateFrom < DateTime.UtcNow.Date)
            {
                return Result.Failure<OrderDto>("Reservation date from must be a future date");
            }

            // Check date conflicts
            var hasConflict = await _context.ReservedVehiclesPerDays
                .Include(rv => rv.Order)
                .AnyAsync(rv => rv.SubCategoryId == request.SubCategoryId
                    && rv.State == ReservedVehicleState.StillBooked
                    && rv.Order.OrderState != OrderState.Completed
                    && ((rv.DateFrom <= request.ReservationDateTo && rv.DateTo >= request.ReservationDateFrom)), cancellationToken);

            if (hasConflict)
            {
                return Result.Failure<OrderDto>("Selected dates are not available");
            }

            // Calculate totals
            var orderSubTotal = OrderCalculationService.CalculateOrderSubTotal(subCategory.Price, request.VehiclesCount);
            
            // Calculate individual fees
            var deliveryFeesAmount = (city.DeliveryFees ?? 0) * request.VehiclesCount;
            var serviceFeesAmount = (city.ServiceFees ?? 0) * orderSubTotal / 100;
            var urgentFeesAmount = (request.IsUrgent && city.UrgentDelivery.HasValue) ? city.UrgentDelivery.Value : 0;
            
            var orderTotal = OrderCalculationService.CalculateOrderTotal(
                orderSubTotal,
                city.DeliveryFees,
                city.ServiceFees,
                city.UrgentDelivery,
                request.VehiclesCount,
                request.IsUrgent);

            // Validate total match (tolerance: 0.50)
            if (!OrderCalculationService.ValidateTotalMatch(orderTotal, request.MobileTotal, 0.50m))
            {
                return Result.Failure<OrderDto>("There is a mistake in calculation");
            }

            // Use mobile total if within tolerance
            var finalTotal = request.MobileTotal;

            // Generate unique OrderCode
            var orderCode = GenerateOrderCode();
            var maxRetries = 10;
            var retryCount = 0;

            while (await _context.Orders.AnyAsync(o => o.OrderCode == orderCode, cancellationToken) && retryCount < maxRetries)
            {
                orderCode = GenerateOrderCode();
                retryCount++;
            }

            if (retryCount >= maxRetries)
            {
                return Result.Failure<OrderDto>("Failed to generate unique order code. Please try again.");
            }

            try
            {
                var order = Domain.Models.Order.Create(
                    customerId,
                    request.SubCategoryId,
                    request.CityId,
                    request.ReservationDateFrom,
                    request.ReservationDateTo,
                    request.VehiclesCount,
                    orderSubTotal,
                    finalTotal,
                    request.PassportImage,
                    request.HotelName,
                    request.HotelAddress,
                    request.PaymentMethodId,
                    request.IsUrgent,
                    orderCode,
                    request.HotelPhone,
                    request.Notes,
                    _userSession.UserName ?? "System"
                );

                await _context.Orders.AddAsync(order);
                
                await _context.SaveChangesAsync(cancellationToken);

                var orderTotals = Domain.Models.OrderTotals.Create(
                    order.OrderId,
                    orderSubTotal,
                    serviceFeesAmount,
                    deliveryFeesAmount,
                    urgentFeesAmount,
                    finalTotal
                );

                var orderPayment = Domain.Models.OrderPayment.Create(
                    order.OrderId,
                    request.PaymentMethodId,
                    finalTotal,
                    _userSession.UserName ?? "System"
                );

                await _context.OrderTotals.AddAsync(orderTotals);
                await _context.OrderPayments.AddAsync(orderPayment);

                string? payPalApproveLink = null;
                string? payPalOrderId = null;

                if (request.PaymentMethodId == (int)PaymentMethod.PayPal)
                {
                    var createOrderResult = await _payPalService.CreatePayPalOrderAsync(order.OrderCode, finalTotal, "EUR");
                    
                    if (createOrderResult.IsSuccess)
                    {
                        payPalApproveLink = createOrderResult.ApproveLink;
                        payPalOrderId = createOrderResult.PayPalOrderId;
                    }
                    else
                    {
                        orderPayment.MarkAsFailed(_userSession.UserName ?? "System");
                        await _context.SaveChangesAsync(cancellationToken);
                        return Result.Failure<OrderDto>($"Failed to create PayPal order: {createOrderResult.ErrorMessage ?? "Unknown error"}");
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                var orderDto = new OrderDto
                {
                    OrderId = order.OrderId,
                    OrderCode = order.OrderCode,
                    CustomerId = order.CustomerId,
                    CustomerName = customer.FullName,
                    SubCategoryId = order.SubCategoryId,
                    SubCategoryName = subCategory.Name,
                    CityId = order.CityId,
                    CityName = city.Name,
                    ReservationDateFrom = order.ReservationDateFrom,
                    ReservationDateTo = order.ReservationDateTo,
                    VehiclesCount = order.VehiclesCount,
                    OrderSubTotal = order.OrderSubTotal,
                    OrderTotal = order.OrderTotal,
                    Notes = order.Notes,
                    HotelName = order.HotelName,
                    HotelAddress = order.HotelAddress,
                    HotelPhone = order.HotelPhone,
                    IsUrgent = order.IsUrgent,
                    PaymentMethod = (PaymentMethod)order.PaymentMethodId,
                    OrderState = order.OrderState,
                    CreatedDate = order.CreatedDate,
                    PayPalApproveLink = payPalApproveLink,
                    PayPalOrderId = payPalOrderId
                };

                return Result.Success(orderDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<OrderDto>($"Error creating order: {ex.Message}");
            }
        }

        private string GenerateOrderCode()
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = GenerateRandomAlphanumeric(6);
            return $"ORD-{datePart}-{randomPart}";
        }

        private string GenerateRandomAlphanumeric(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

