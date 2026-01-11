namespace Application.Features.Order.DTOs
{
    public class VehicleUtilizationReportDto
    {
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string VehicleCode { get; set; } = string.Empty;
        public int TotalDaysBooked { get; set; }
        public int TotalOrders { get; set; }
        public decimal UtilizationPercentage { get; set; }
    }
}

