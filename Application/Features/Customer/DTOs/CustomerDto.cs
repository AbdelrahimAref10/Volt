using Domain.Enums;

namespace Application.Features.Customer.DTOs
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string NationalNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public CustomerState State { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

