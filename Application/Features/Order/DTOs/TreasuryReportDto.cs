namespace Application.Features.Order.DTOs
{
    public class TreasuryReportDto
    {
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Balance { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}

