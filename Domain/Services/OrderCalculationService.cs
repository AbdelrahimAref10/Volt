using Domain.Models;

namespace Domain.Services
{
    public class OrderCalculationService
    {
        /// <summary>
        /// Calculates the order subtotal (price * count)
        /// </summary>
        public static decimal CalculateOrderSubTotal(decimal subCategoryPrice, int vehiclesCount)
        {
            if (subCategoryPrice < 0)
                throw new ArgumentException("SubCategory price cannot be negative", nameof(subCategoryPrice));

            if (vehiclesCount <= 0)
                throw new ArgumentException("Vehicles count must be greater than zero", nameof(vehiclesCount));

            return subCategoryPrice * vehiclesCount;
        }

        /// <summary>
        /// Calculates the full order total with all fees
        /// Formula: SubTotal + (DeliveryFees * Count) + (ServiceFees * SubTotal / 100) + UrgentFees (if urgent)
        /// </summary>
        public static decimal CalculateOrderTotal(
            decimal subTotal,
            decimal? deliveryFees,
            decimal? serviceFees,
            decimal? urgentDelivery,
            int vehiclesCount,
            bool isUrgent)
        {
            if (subTotal < 0)
                throw new ArgumentException("SubTotal cannot be negative", nameof(subTotal));

            if (vehiclesCount <= 0)
                throw new ArgumentException("Vehicles count must be greater than zero", nameof(vehiclesCount));

            decimal total = subTotal;

            // Add delivery fees (per vehicle)
            if (deliveryFees.HasValue && deliveryFees.Value > 0)
            {
                total += deliveryFees.Value * vehiclesCount;
            }

            // Add service fees (percentage of subtotal)
            if (serviceFees.HasValue && serviceFees.Value > 0)
            {
                var serviceFeesAmount = serviceFees.Value * subTotal / 100;
                total += serviceFeesAmount;
            }

            // Add urgent delivery fee (if urgent)
            if (isUrgent && urgentDelivery.HasValue && urgentDelivery.Value > 0)
            {
                total += urgentDelivery.Value;
            }

            return total;
        }

        /// <summary>
        /// Validates that the backend total matches the mobile total within tolerance
        /// </summary>
        public static bool ValidateTotalMatch(decimal backendTotal, decimal mobileTotal, decimal tolerance = 0.50m)
        {
            var difference = Math.Abs(backendTotal - mobileTotal);
            return difference <= tolerance;
        }

        /// <summary>
        /// Calculates cancellation fee based on city and order age (4 days policy)
        /// </summary>
        public static decimal? CalculateCancellationFee(City city, DateTime orderCreatedDate)
        {
            if (city == null)
                throw new ArgumentNullException(nameof(city));

            var daysSinceCreation = (DateTime.UtcNow - orderCreatedDate).TotalDays;

            // If order created within 4 days, no cancellation fee
            if (daysSinceCreation <= 4)
            {
                return null;
            }

            // If order created more than 4 days ago, apply cancellation fee
            return city.CancellationFees;
        }
    }
}

