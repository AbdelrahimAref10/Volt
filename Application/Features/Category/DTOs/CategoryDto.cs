namespace Application.Features.Category.DTOs
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int VehicleCount { get; set; }
    }

    public class CategoryLookupDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}


