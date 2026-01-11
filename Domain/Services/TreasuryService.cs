using Domain.Models;

namespace Domain.Services
{
    public class TreasuryService
    {
        /// <summary>
        /// Adds order revenue to treasury (called when order is completed)
        /// </summary>
        public static void AddOrderRevenue(CompanyTreasury treasury, decimal amount, string? modifiedBy = null)
        {
            if (treasury == null)
                throw new ArgumentNullException(nameof(treasury));

            treasury.AddRevenue(amount, modifiedBy);
        }

        /// <summary>
        /// Adds cancellation fee to treasury (called when cancellation fee is paid)
        /// </summary>
        public static void AddCancellationFee(CompanyTreasury treasury, decimal amount, string? modifiedBy = null)
        {
            if (treasury == null)
                throw new ArgumentNullException(nameof(treasury));

            treasury.AddCancellationFee(amount, modifiedBy);
        }

        /// <summary>
        /// Gets current treasury balance
        /// </summary>
        public static decimal GetTreasuryBalance(CompanyTreasury treasury)
        {
            if (treasury == null)
                throw new ArgumentNullException(nameof(treasury));

            return treasury.GetBalance();
        }
    }
}

