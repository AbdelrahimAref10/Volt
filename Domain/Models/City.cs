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
        public decimal? DeliveryFees { get; private set; }
        public decimal? UrgentDelivery { get; private set; }
        public decimal? ServiceFees { get; private set; } // Percentage value (e.g., 5.0 means 5%)
        public decimal? CancellationFees { get; private set; }

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

        public void UpdateFees(
            decimal? deliveryFees,
            decimal? urgentDelivery,
            decimal? serviceFees,
            decimal? cancellationFees,
            string? modifiedBy = null)
        {
            if (deliveryFees.HasValue && deliveryFees.Value < 0)
                throw new ArgumentException("Delivery fees cannot be negative", nameof(deliveryFees));

            if (urgentDelivery.HasValue && urgentDelivery.Value < 0)
                throw new ArgumentException("Urgent delivery fees cannot be negative", nameof(urgentDelivery));

            if (serviceFees.HasValue && serviceFees.Value < 0)
                throw new ArgumentException("Service fees cannot be negative", nameof(serviceFees));

            if (cancellationFees.HasValue && cancellationFees.Value < 0)
                throw new ArgumentException("Cancellation fees cannot be negative", nameof(cancellationFees));

            DeliveryFees = deliveryFees;
            UrgentDelivery = urgentDelivery;
            ServiceFees = serviceFees;
            CancellationFees = cancellationFees;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }
    }
}

