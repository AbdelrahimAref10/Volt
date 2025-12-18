using Domain.Common;

namespace Domain.Models
{
    public class Category : IAuditable
    {
        // Private setters for encapsulation
        public int CategoryId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string? ImageUrl { get; private set; }
        public bool IsActive { get; private set; } = true;

        // Navigation property
        public ICollection<Vehicle> Vehicles { get; private set; } = new List<Vehicle>();

        // Audit properties
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Private constructor for EF Core
        private Category() { }

        // Factory method for creating categories
        public static Category Create(
            string name,
            string description,
            string? imageUrl = null,
            string? createdBy = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name cannot be empty", nameof(name));

            return new Category
            {
                Name = name.Trim(),
                Description = description ?? string.Empty,
                ImageUrl = imageUrl,
                IsActive = true,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };
        }

        // Domain methods
        public void Update(string name, string description, string? imageUrl = null, string? modifiedBy = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name cannot be empty", nameof(name));

            Name = name.Trim();
            Description = description ?? string.Empty;
            ImageUrl = imageUrl;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void Deactivate(string? modifiedBy = null)
        {
            IsActive = false;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void Activate(string? modifiedBy = null)
        {
            IsActive = true;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void UpdateImage(string? imageUrl, string? modifiedBy = null)
        {
            ImageUrl = imageUrl;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }
    }
}


