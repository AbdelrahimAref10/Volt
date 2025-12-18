using Domain.Common;

namespace Domain.Models
{
    public class Vehicle : IAuditable
    {
        // Private setters for encapsulation
        public int VehicleId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? ImageUrl { get; private set; }
        public decimal PricePerHour { get; private set; }
        public string Status { get; private set; } = string.Empty; // "Available", "Under Maintenance", "Rented"
        public DateTime? CreatedThisMonth { get; private set; }

        // Foreign key and navigation property
        public int CategoryId { get; private set; }
        public Category Category { get; private set; } = null!;

        // Audit properties
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Private constructor for EF Core
        private Vehicle() { }

        // Factory method for creating vehicles
        public static Vehicle Create(
            string name,
            int categoryId,
            decimal pricePerHour,
            string status,
            string? imageUrl = null,
            string? createdBy = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Vehicle name cannot be empty", nameof(name));

            if (categoryId <= 0)
                throw new ArgumentException("Category ID must be greater than zero", nameof(categoryId));

            if (pricePerHour < 0)
                throw new ArgumentException("Price per hour cannot be negative", nameof(pricePerHour));

            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be empty", nameof(status));

            var validStatuses = new[] { "Available", "Under Maintenance", "Rented" };
            if (!validStatuses.Contains(status))
                throw new ArgumentException($"Status must be one of: {string.Join(", ", validStatuses)}", nameof(status));

            return new Vehicle
            {
                Name = name.Trim(),
                CategoryId = categoryId,
                PricePerHour = pricePerHour,
                Status = status,
                ImageUrl = imageUrl,
                CreatedThisMonth = DateTime.UtcNow, // Track if created this month
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };
        }

        // Domain methods
        public void Update(
            string name,
            int categoryId,
            decimal pricePerHour,
            string status,
            string? imageUrl = null,
            string? modifiedBy = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Vehicle name cannot be empty", nameof(name));

            if (categoryId <= 0)
                throw new ArgumentException("Category ID must be greater than zero", nameof(categoryId));

            if (pricePerHour < 0)
                throw new ArgumentException("Price per hour cannot be negative", nameof(pricePerHour));

            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be empty", nameof(status));

            var validStatuses = new[] { "Available", "Under Maintenance", "Rented" };
            if (!validStatuses.Contains(status))
                throw new ArgumentException($"Status must be one of: {string.Join(", ", validStatuses)}", nameof(status));

            Name = name.Trim();
            CategoryId = categoryId;
            PricePerHour = pricePerHour;
            Status = status;
            ImageUrl = imageUrl;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void UpdateStatus(string status, string? modifiedBy = null)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be empty", nameof(status));

            var validStatuses = new[] { "Available", "Under Maintenance", "Rented" };
            if (!validStatuses.Contains(status))
                throw new ArgumentException($"Status must be one of: {string.Join(", ", validStatuses)}", nameof(status));

            Status = status;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void UpdateImage(string? imageUrl, string? modifiedBy = null)
        {
            ImageUrl = imageUrl;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public bool IsNewThisMonth => CreatedThisMonth.HasValue && 
            CreatedThisMonth.Value.Year == DateTime.UtcNow.Year && 
            CreatedThisMonth.Value.Month == DateTime.UtcNow.Month;
    }
}


