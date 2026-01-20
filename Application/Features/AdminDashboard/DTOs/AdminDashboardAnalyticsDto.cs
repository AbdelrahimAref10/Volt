using Application.Features.Order.DTOs;
using Application.Features.Vehicle.Query.GetVehicleStatisticsQuery;

namespace Application.Features.AdminDashboard.DTOs
{
    public class AdminDashboardAnalyticsDto
    {
        // Overall Statistics
        public OverallStatisticsDto OverallStatistics { get; set; } = new();
        
        // Order Statistics
        public OrderStatisticsDto OrderStatistics { get; set; } = new();
        
        // Revenue Analytics
        public RevenueAnalyticsDto RevenueAnalytics { get; set; } = new();
        
        // Vehicle Statistics
        public VehicleStatisticsDto VehicleStatistics { get; set; } = new();
        
        // Customer Statistics
        public CustomerStatisticsDto CustomerStatistics { get; set; } = new();
        
        // Treasury Information
        public TreasuryReportDto TreasuryBalance { get; set; } = new();
        
        // Cancellation Statistics
        public CancellationReportDto CancellationStatistics { get; set; } = new();
        
        // Payment Method Breakdown
        public List<PaymentMethodBreakdownDto> PaymentMethodBreakdown { get; set; } = new();
        
        // Top Performing Categories
        public List<TopCategoryDto> TopCategories { get; set; } = new();
        
        // Recent Orders
        public List<RecentOrderDto> RecentOrders { get; set; } = new();
        
        // Orders by State
        public List<OrdersByStateReportDto> OrdersByState { get; set; } = new();
    }

    public class OverallStatisticsDto
    {
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalVehicles { get; set; }
        public int TotalCategories { get; set; }
        public int TotalSubCategories { get; set; }
        public int TotalCities { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int ActiveCustomers { get; set; }
        public int AvailableVehicles { get; set; }
    }

    public class OrderStatisticsDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int OnWayOrders { get; set; }
        public int CustomerReceivedOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int OrdersToday { get; set; }
        public int OrdersThisWeek { get; set; }
        public int OrdersThisMonth { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class RevenueAnalyticsDto
    {
        public decimal TodayRevenue { get; set; }
        public decimal ThisWeekRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal ThisQuarterRevenue { get; set; }
        public decimal ThisYearRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<RevenueReportDto> RevenueByPeriod { get; set; } = new();
    }

    public class CustomerStatisticsDto
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int InactiveCustomers { get; set; }
        public int BlockedCustomers { get; set; }
        public int NewCustomersToday { get; set; }
        public int NewCustomersThisWeek { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int IndividualCustomers { get; set; }
        public int InstitutionCustomers { get; set; }
    }

    public class PaymentMethodBreakdownDto
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class TopCategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public int VehicleCount { get; set; }
    }

    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string SubCategoryName { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
        public string OrderState { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
