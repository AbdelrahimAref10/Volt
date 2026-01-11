using Domain.Common;

namespace Domain.Models
{
    public class CompanyTreasury : IAuditable
    {
        // Private setters for encapsulation
        public int Id { get; private set; }
        public decimal TotalRevenue { get; private set; } = 0;
        public decimal TotalCancellationFees { get; private set; } = 0;
        public DateTime LastUpdated { get; private set; }

        // Audit properties
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Private constructor for EF Core
        private CompanyTreasury() { }

        // Factory method for creating treasury
        public static CompanyTreasury Create(string? createdBy = null)
        {
            return new CompanyTreasury
            {
                TotalRevenue = 0,
                TotalCancellationFees = 0,
                LastUpdated = DateTime.UtcNow,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };
        }

        // Domain methods
        public void AddRevenue(decimal amount, string? modifiedBy = null)
        {
            if (amount < 0)
                throw new ArgumentException("Revenue amount cannot be negative", nameof(amount));

            TotalRevenue += amount;
            LastUpdated = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void AddCancellationFee(decimal amount, string? modifiedBy = null)
        {
            if (amount < 0)
                throw new ArgumentException("Cancellation fee amount cannot be negative", nameof(amount));

            TotalCancellationFees += amount;
            LastUpdated = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public decimal GetBalance()
        {
            return TotalRevenue + TotalCancellationFees;
        }
    }
}

