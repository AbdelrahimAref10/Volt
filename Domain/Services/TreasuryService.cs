using Domain.Models;

namespace Domain.Services
{
    public class TreasuryService
    {
        /// <summary>
        /// Creates a treasury record for cash payment when customer receives vehicle
        /// </summary>
        public static CompanyTreasury CreateCashPaymentRecord(
            decimal amount,
            string orderCode,
            string? createdBy = null)
        {
            return CompanyTreasury.Create(
                debitAmount: amount,
                creditAmount: 0,
                descriptionAr: $"دفع نقدي للطلب {orderCode}",
                descriptionEng: $"Cash payment for order {orderCode}",
                createdBy: createdBy);
        }

        /// <summary>
        /// Creates a treasury record for PayPal payment when payment is successful
        /// </summary>
        public static CompanyTreasury CreatePayPalPaymentRecord(
            decimal amount,
            string orderCode,
            string? createdBy = null)
        {
            return CompanyTreasury.Create(
                debitAmount: 0,
                creditAmount: amount,
                descriptionAr: $"دفع PayPal للطلب {orderCode}",
                descriptionEng: $"PayPal payment for order {orderCode}",
                createdBy: createdBy);
        }

        /// <summary>
        /// Creates a treasury record for cancellation fee
        /// </summary>
        public static CompanyTreasury CreateCancellationFeeRecord(
            decimal amount,
            string orderCode,
            string? createdBy = null)
        {
            return CompanyTreasury.Create(
                debitAmount: 0,
                creditAmount: amount,
                descriptionAr: $"رسوم إلغاء للطلب {orderCode}",
                descriptionEng: $"Cancellation fee for order {orderCode}",
                createdBy: createdBy);
        }
    }
}

