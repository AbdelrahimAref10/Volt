namespace Application.Features.Vehicle.DTOs
{
    public class VehicleDto
    {
        public int VehicleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal PricePerHour { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsNewThisMonth { get; set; }
    }
}


