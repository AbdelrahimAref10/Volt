namespace Application.Features.Order.DTOs
{
    public class OrderVehicleDto
    {
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string VehicleCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}

