namespace Application.Features.Order.DTOs
{
    public class CityFeesDto
    {
        public int CityId { get; set; }
        public decimal? ServiceFees { get; set; } // Percentage value
        public decimal? DeliveryFees { get; set; }
        public decimal? UrgentFees { get; set; }
        public decimal? CancellationFees { get; set; }
    }
}

