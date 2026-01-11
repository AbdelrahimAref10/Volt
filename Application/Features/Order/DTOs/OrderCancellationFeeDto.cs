using Domain.Enums;

namespace Application.Features.Order.DTOs
{
    public class OrderCancellationFeeDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public CancellationFeeState State { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

