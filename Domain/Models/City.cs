using Domain.Common;

namespace Domain.Models
{
    public class City : IAuditable
    {
        // Private setters for encapsulation
        public int CityId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;

        // Navigation property - one City has many Customers
        public ICollection<Customer> Customers { get; private set; } = new List<Customer>();

        // Audit properties
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Private constructor for EF Core
        private City() { }

        // Factory method for creating cities
        public static City Create(
            string name,
            string? description = null,
            string? createdBy = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("City name cannot be empty", nameof(name));

            return new City
            {
                Name = name.Trim(),
                Description = description,
                IsActive = true,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };
        }

        // Domain methods
        public void Update(string name, string? description = null, string? modifiedBy = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("City name cannot be empty", nameof(name));

            Name = name.Trim();
            Description = description;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void Activate(string? modifiedBy = null)
        {
            IsActive = true;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void Deactivate(string? modifiedBy = null)
        {
            IsActive = false;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }
    }
}

