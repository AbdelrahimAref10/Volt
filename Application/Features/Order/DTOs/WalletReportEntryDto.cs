using Domain.Enums;

namespace Application.Features.Order.DTOs
{
    /// <summary>
    /// Wallet entry for admin wallet report (includes customer display info).
    /// </summary>
    public class WalletReportEntryDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerMobileNumber { get; set; } = string.Empty;
        public int? OrderId { get; set; }
        public decimal Withdraw { get; set; }
        public decimal Deposit { get; set; }
        public string Description { get; set; } = string.Empty;
        public WalletType Type { get; set; }
        public CustomerWalletState State { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
