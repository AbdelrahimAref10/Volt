namespace Application.Features.Order.DTOs
{
    public class CancellationReportDto
    {
        public int TotalCancelledOrders { get; set; }
        public decimal TotalCancellationFees { get; set; }
        public decimal PaidCancellationFees { get; set; }
        public decimal UnpaidCancellationFees { get; set; }
    }
}

