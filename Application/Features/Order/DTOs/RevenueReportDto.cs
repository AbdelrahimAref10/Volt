namespace Application.Features.Order.DTOs
{
    public class RevenueReportDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public string Period { get; set; } = string.Empty; // "month", "quarter", "year"
    }
}

