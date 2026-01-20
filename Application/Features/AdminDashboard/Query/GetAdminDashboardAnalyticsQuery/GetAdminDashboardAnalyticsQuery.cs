using Application.Features.AdminDashboard.DTOs;
using Application.Features.Order.DTOs;
using Application.Features.Order.Query.Reports.CancellationReportQuery;
using Application.Features.Order.Query.Reports.OrdersByStateReportQuery;
using Application.Features.Order.Query.Reports.RevenueByPeriodReportQuery;
using Application.Features.Order.Query.Reports.RevenueReportQuery;
using Application.Features.Order.Query.Reports.TreasuryBalanceReportQuery;
using Application.Features.Vehicle.Query.GetVehicleStatisticsQuery;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminDashboard.Query.GetAdminDashboardAnalyticsQuery
{
    public record GetAdminDashboardAnalyticsQuery : IRequest<Result<AdminDashboardAnalyticsDto>>
    {
        public int RecentOrdersCount { get; set; } = 10;
        public int TopCategoriesCount { get; set; } = 5;
        public int RevenuePeriodsCount { get; set; } = 6; // Last 6 months
    }

    public class GetAdminDashboardAnalyticsQueryHandler : IRequestHandler<GetAdminDashboardAnalyticsQuery, Result<AdminDashboardAnalyticsDto>>
    {
        private readonly DatabaseContext _context;
        private readonly IMediator _mediator;

        public GetAdminDashboardAnalyticsQueryHandler(DatabaseContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<Result<AdminDashboardAnalyticsDto>> Handle(GetAdminDashboardAnalyticsQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfQuarter = new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1);
            var startOfYear = new DateTime(now.Year, 1, 1);

            var analytics = new AdminDashboardAnalyticsDto();

            // Overall Statistics
            analytics.OverallStatistics = await GetOverallStatistics(now, cancellationToken);

            // Order Statistics
            analytics.OrderStatistics = await GetOrderStatistics(now, today, startOfWeek, startOfMonth, cancellationToken);

            // Revenue Analytics
            analytics.RevenueAnalytics = await GetRevenueAnalytics(now, today, startOfWeek, startOfMonth, startOfQuarter, startOfYear, request.RevenuePeriodsCount, cancellationToken);

            // Vehicle Statistics
            var vehicleStatsResult = await _mediator.Send(new GetVehicleStatisticsQuery(), cancellationToken);
            if (vehicleStatsResult.IsSuccess)
            {
                analytics.VehicleStatistics = vehicleStatsResult.Value;
            }

            // Customer Statistics
            analytics.CustomerStatistics = await GetCustomerStatistics(now, today, startOfWeek, startOfMonth, cancellationToken);

            // Treasury Balance
            var treasuryResult = await _mediator.Send(new TreasuryBalanceReportQuery(), cancellationToken);
            if (treasuryResult.IsSuccess)
            {
                analytics.TreasuryBalance = treasuryResult.Value;
            }

            // Cancellation Statistics
            var cancellationResult = await _mediator.Send(new CancellationReportQuery(), cancellationToken);
            if (cancellationResult.IsSuccess)
            {
                analytics.CancellationStatistics = cancellationResult.Value;
            }

            // Orders by State
            var ordersByStateResult = await _mediator.Send(new OrdersByStateReportQuery(), cancellationToken);
            if (ordersByStateResult.IsSuccess)
            {
                analytics.OrdersByState = ordersByStateResult.Value;
            }

            // Payment Method Breakdown
            analytics.PaymentMethodBreakdown = await GetPaymentMethodBreakdown(cancellationToken);

            // Top Categories
            analytics.TopCategories = await GetTopCategories(request.TopCategoriesCount, cancellationToken);

            // Recent Orders
            analytics.RecentOrders = await GetRecentOrders(request.RecentOrdersCount, cancellationToken);

            return Result.Success(analytics);
        }

        private async Task<OverallStatisticsDto> GetOverallStatistics(DateTime now, CancellationToken cancellationToken)
        {
            var totalOrders = await _context.Orders.CountAsync(cancellationToken);
            var totalCustomers = await _context.Customers.CountAsync(cancellationToken);
            var totalVehicles = await _context.Vehicles.CountAsync(cancellationToken);
            var totalCategories = await _context.Categories.CountAsync(cancellationToken);
            var totalSubCategories = await _context.SubCategories.CountAsync(cancellationToken);
            var totalCities = await _context.Cities.CountAsync(cancellationToken);
            
            var completedOrders = await _context.Orders
                .Include(o => o.OrderPayments)
                .Where(o => o.OrderState == OrderState.Completed)
                .ToListAsync(cancellationToken);
            
            var totalRevenue = completedOrders
                .SelectMany(o => o.OrderPayments)
                .Where(p => p.State == PaymentState.Paid)
                .Sum(p => p.Total);

            var pendingOrders = await _context.Orders.CountAsync(o => o.OrderState == OrderState.Pending, cancellationToken);
            var activeCustomers = await _context.Customers.CountAsync(c => c.State == CustomerState.Active, cancellationToken);
            var availableVehicles = await _context.Vehicles.CountAsync(v => v.Status == "Available", cancellationToken);

            return new OverallStatisticsDto
            {
                TotalOrders = totalOrders,
                TotalCustomers = totalCustomers,
                TotalVehicles = totalVehicles,
                TotalCategories = totalCategories,
                TotalSubCategories = totalSubCategories,
                TotalCities = totalCities,
                TotalRevenue = totalRevenue,
                PendingOrders = pendingOrders,
                ActiveCustomers = activeCustomers,
                AvailableVehicles = availableVehicles
            };
        }

        private async Task<OrderStatisticsDto> GetOrderStatistics(DateTime now, DateTime today, DateTime startOfWeek, DateTime startOfMonth, CancellationToken cancellationToken)
        {
            var allOrders = _context.Orders.AsQueryable();
            
            var totalOrders = await allOrders.CountAsync(cancellationToken);
            var pendingOrders = await allOrders.CountAsync(o => o.OrderState == OrderState.Pending, cancellationToken);
            var confirmedOrders = await allOrders.CountAsync(o => o.OrderState == OrderState.Confirmed, cancellationToken);
            var onWayOrders = await allOrders.CountAsync(o => o.OrderState == OrderState.OnWay, cancellationToken);
            var customerReceivedOrders = await allOrders.CountAsync(o => o.OrderState == OrderState.CustomerReceived, cancellationToken);
            var completedOrders = await allOrders.CountAsync(o => o.OrderState == OrderState.Completed, cancellationToken);
            var cancelledOrders = await _context.OrderCancellationFees
                .CountAsync(cancellationToken);
            
            var ordersToday = await allOrders.CountAsync(o => o.CreatedDate >= today && o.CreatedDate < today.AddDays(1), cancellationToken);
            var ordersThisWeek = await allOrders.CountAsync(o => o.CreatedDate >= startOfWeek, cancellationToken);
            var ordersThisMonth = await allOrders.CountAsync(o => o.CreatedDate >= startOfMonth, cancellationToken);

            var completedOrdersList = await _context.Orders
                .Include(o => o.OrderPayments)
                .Where(o => o.OrderState == OrderState.Completed)
                .ToListAsync(cancellationToken);

            var totalRevenue = completedOrdersList
                .SelectMany(o => o.OrderPayments)
                .Where(p => p.State == PaymentState.Paid)
                .Sum(p => p.Total);

            var averageOrderValue = completedOrders > 0 ? totalRevenue / completedOrders : 0;

            return new OrderStatisticsDto
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                ConfirmedOrders = confirmedOrders,
                OnWayOrders = onWayOrders,
                CustomerReceivedOrders = customerReceivedOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                OrdersToday = ordersToday,
                OrdersThisWeek = ordersThisWeek,
                OrdersThisMonth = ordersThisMonth,
                AverageOrderValue = averageOrderValue
            };
        }

        private async Task<RevenueAnalyticsDto> GetRevenueAnalytics(DateTime now, DateTime today, DateTime startOfWeek, DateTime startOfMonth, DateTime startOfQuarter, DateTime startOfYear, int periodsCount, CancellationToken cancellationToken)
        {
            var completedOrders = await _context.Orders
                .Include(o => o.OrderPayments)
                .Where(o => o.OrderState == OrderState.Completed)
                .ToListAsync(cancellationToken);

            var paidPayments = completedOrders
                .SelectMany(o => o.OrderPayments)
                .Where(p => p.State == PaymentState.Paid)
                .ToList();

            var todayRevenue = paidPayments
                .Where(p => p.CreatedDate >= today && p.CreatedDate < today.AddDays(1))
                .Sum(p => p.Total);

            var thisWeekRevenue = paidPayments
                .Where(p => p.CreatedDate >= startOfWeek)
                .Sum(p => p.Total);

            var thisMonthRevenue = paidPayments
                .Where(p => p.CreatedDate >= startOfMonth)
                .Sum(p => p.Total);

            var thisQuarterRevenue = paidPayments
                .Where(p => p.CreatedDate >= startOfQuarter)
                .Sum(p => p.Total);

            var thisYearRevenue = paidPayments
                .Where(p => p.CreatedDate >= startOfYear)
                .Sum(p => p.Total);

            var totalRevenue = paidPayments.Sum(p => p.Total);

            // Get revenue by period (last N months)
            var revenueByPeriod = new List<RevenueReportDto>();
            for (int i = periodsCount - 1; i >= 0; i--)
            {
                var periodStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                var periodEnd = periodStart.AddMonths(1).AddDays(-1);
                
                var periodRevenue = paidPayments
                    .Where(p => p.CreatedDate >= periodStart && p.CreatedDate <= periodEnd)
                    .Sum(p => p.Total);

                var periodOrders = completedOrders
                    .Count(o => o.CreatedDate >= periodStart && o.CreatedDate <= periodEnd);

                revenueByPeriod.Add(new RevenueReportDto
                {
                    TotalRevenue = periodRevenue,
                    TotalOrders = periodOrders,
                    AverageOrderValue = periodOrders > 0 ? periodRevenue / periodOrders : 0,
                    Period = periodStart.ToString("MMM yyyy")
                });
            }

            return new RevenueAnalyticsDto
            {
                TodayRevenue = todayRevenue,
                ThisWeekRevenue = thisWeekRevenue,
                ThisMonthRevenue = thisMonthRevenue,
                ThisQuarterRevenue = thisQuarterRevenue,
                ThisYearRevenue = thisYearRevenue,
                TotalRevenue = totalRevenue,
                RevenueByPeriod = revenueByPeriod
            };
        }

        private async Task<CustomerStatisticsDto> GetCustomerStatistics(DateTime now, DateTime today, DateTime startOfWeek, DateTime startOfMonth, CancellationToken cancellationToken)
        {
            var allCustomers = _context.Customers.AsQueryable();

            var totalCustomers = await allCustomers.CountAsync(cancellationToken);
            var activeCustomers = await allCustomers.CountAsync(c => c.State == CustomerState.Active, cancellationToken);
            var inactiveCustomers = await allCustomers.CountAsync(c => c.State == CustomerState.InActive, cancellationToken);
            var blockedCustomers = await allCustomers.CountAsync(c => c.State == CustomerState.Blocked, cancellationToken);
            
            var newCustomersToday = await allCustomers.CountAsync(c => c.CreatedDate >= today && c.CreatedDate < today.AddDays(1), cancellationToken);
            var newCustomersThisWeek = await allCustomers.CountAsync(c => c.CreatedDate >= startOfWeek, cancellationToken);
            var newCustomersThisMonth = await allCustomers.CountAsync(c => c.CreatedDate >= startOfMonth, cancellationToken);
            
            var individualCustomers = await allCustomers.CountAsync(c => c.RegisterAs == 0, cancellationToken);
            var institutionCustomers = await allCustomers.CountAsync(c => c.RegisterAs == 1, cancellationToken);

            return new CustomerStatisticsDto
            {
                TotalCustomers = totalCustomers,
                ActiveCustomers = activeCustomers,
                InactiveCustomers = inactiveCustomers,
                BlockedCustomers = blockedCustomers,
                NewCustomersToday = newCustomersToday,
                NewCustomersThisWeek = newCustomersThisWeek,
                NewCustomersThisMonth = newCustomersThisMonth,
                IndividualCustomers = individualCustomers,
                InstitutionCustomers = institutionCustomers
            };
        }

        private async Task<List<PaymentMethodBreakdownDto>> GetPaymentMethodBreakdown(CancellationToken cancellationToken)
        {
            var completedOrders = await _context.Orders
                .Include(o => o.OrderPayments)
                .Where(o => o.OrderState == OrderState.Completed)
                .ToListAsync(cancellationToken);

            var totalRevenue = completedOrders
                .SelectMany(o => o.OrderPayments)
                .Where(p => p.State == PaymentState.Paid)
                .Sum(p => p.Total);

            var cashOrders = completedOrders.Where(o => o.PaymentMethodId == (int)PaymentMethod.Cash).ToList();
            var paypalOrders = completedOrders.Where(o => o.PaymentMethodId == (int)PaymentMethod.PayPal).ToList();

            var cashRevenue = cashOrders
                .SelectMany(o => o.OrderPayments)
                .Where(p => p.State == PaymentState.Paid)
                .Sum(p => p.Total);

            var paypalRevenue = paypalOrders
                .SelectMany(o => o.OrderPayments)
                .Where(p => p.State == PaymentState.Paid)
                .Sum(p => p.Total);

            var breakdown = new List<PaymentMethodBreakdownDto>();

            if (cashOrders.Count > 0 || cashRevenue > 0)
            {
                breakdown.Add(new PaymentMethodBreakdownDto
                {
                    PaymentMethod = "Cash",
                    OrderCount = cashOrders.Count,
                    TotalAmount = cashRevenue,
                    Percentage = totalRevenue > 0 ? (cashRevenue / totalRevenue) * 100 : 0
                });
            }

            if (paypalOrders.Count > 0 || paypalRevenue > 0)
            {
                breakdown.Add(new PaymentMethodBreakdownDto
                {
                    PaymentMethod = "PayPal",
                    OrderCount = paypalOrders.Count,
                    TotalAmount = paypalRevenue,
                    Percentage = totalRevenue > 0 ? (paypalRevenue / totalRevenue) * 100 : 0
                });
            }

            return breakdown;
        }

        private async Task<List<TopCategoryDto>> GetTopCategories(int count, CancellationToken cancellationToken)
        {
            var categories = await _context.Categories
                .Include(c => c.SubCategories)
                .ThenInclude(sc => sc.Vehicles)
                .ToListAsync(cancellationToken);

            var subCategoryIds = categories
                .SelectMany(c => c.SubCategories)
                .Select(sc => sc.SubCategoryId)
                .ToList();

            var completedOrders = await _context.Orders
                .Include(o => o.OrderPayments)
                .Where(o => o.OrderState == OrderState.Completed && subCategoryIds.Contains(o.SubCategoryId))
                .ToListAsync(cancellationToken);

            var topCategories = categories
                .Select(c =>
                {
                    var categorySubCategoryIds = c.SubCategories.Select(sc => sc.SubCategoryId).ToList();
                    var categoryOrders = completedOrders.Where(o => categorySubCategoryIds.Contains(o.SubCategoryId)).ToList();
                    
                    return new TopCategoryDto
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.Name,
                        OrderCount = categoryOrders.Count,
                        TotalRevenue = categoryOrders
                            .SelectMany(o => o.OrderPayments)
                            .Where(p => p.State == PaymentState.Paid)
                            .Sum(p => p.Total),
                        VehicleCount = c.SubCategories
                            .SelectMany(sc => sc.Vehicles)
                            .Count()
                    };
                })
                .Where(tc => tc.OrderCount > 0 || tc.TotalRevenue > 0)
                .OrderByDescending(tc => tc.TotalRevenue)
                .Take(count)
                .ToList();

            return topCategories;
        }

        private async Task<List<RecentOrderDto>> GetRecentOrders(int count, CancellationToken cancellationToken)
        {
            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.SubCategory)
                .OrderByDescending(o => o.CreatedDate)
                .Take(count)
                .Select(o => new RecentOrderDto
                {
                    OrderId = o.OrderId,
                    OrderCode = o.OrderCode,
                    CustomerName = o.Customer.FullName,
                    SubCategoryName = o.SubCategory.Name,
                    OrderTotal = o.OrderTotal,
                    OrderState = o.OrderState.ToString(),
                    CreatedDate = o.CreatedDate
                })
                .ToListAsync(cancellationToken);

            return recentOrders;
        }
    }
}
