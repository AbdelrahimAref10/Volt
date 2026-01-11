namespace Application.Features.Order.DTOs
{
    public class TreasuryReportDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCancellationFees { get; set; }
        public decimal Balance { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}

