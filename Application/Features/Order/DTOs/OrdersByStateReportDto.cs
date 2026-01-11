using Domain.Enums;

namespace Application.Features.Order.DTOs
{
    public class OrdersByStateReportDto
    {
        public OrderState OrderState { get; set; }
        public string OrderStateName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}

