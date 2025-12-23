using System;

namespace Application.Features.City.DTOs
{
    public class CityDto
    {
        public int CityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int CustomerCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

