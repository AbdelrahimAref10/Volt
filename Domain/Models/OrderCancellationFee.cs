using Domain.Common;
using Domain.Enums;

namespace Domain.Models
{
    public class OrderCancellationFee : IAuditable
    {
        // Private setters for encapsulation
        public int Id { get; private set; }
        public int CustomerId { get; private set; }
        public int OrderId { get; private set; }
        public decimal Amount { get; private set; }
        public CancellationFeeState State { get; private set; } = CancellationFeeState.NotYet;

        // Navigation properties
        public Customer Customer { get; private set; } = null!;
        public Order Order { get; private set; } = null!;

        // Audit properties
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Private constructor for EF Core
        private OrderCancellationFee() { }

        // Factory method for creating cancellation fees
        public static OrderCancellationFee Create(
            int customerId,
            int orderId,
            decimal amount,
            string? createdBy = null)
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero", nameof(customerId));

            if (orderId <= 0)
                throw new ArgumentException("Order ID must be greater than zero", nameof(orderId));

            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));

            return new OrderCancellationFee
            {
                CustomerId = customerId,
                OrderId = orderId,
                Amount = amount,
                State = CancellationFeeState.NotYet,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };
        }

        // Domain methods
        public void MarkAsPaid(string? modifiedBy = null)
        {
            State = CancellationFeeState.Paid;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }
    }
}

