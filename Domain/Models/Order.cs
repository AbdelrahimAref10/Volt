using Domain.Common;
using Domain.Enums;

namespace Domain.Models
{
    public class Order : IAuditable
    {
        // Private setters for encapsulation
        public int OrderId { get; private set; }
        public string OrderCode { get; private set; } = string.Empty;
        public int CustomerId { get; private set; }
        public int SubCategoryId { get; private set; }
        public int CityId { get; private set; }
        public DateTime ReservationDateFrom { get; private set; }
        public DateTime ReservationDateTo { get; private set; }
        public int VehiclesCount { get; private set; }
        public decimal OrderSubTotal { get; private set; }
        public decimal OrderTotal { get; private set; }
        public string? Notes { get; private set; }
        public string PassportImage { get; private set; } = string.Empty; // Base64 string
        public string HotelName { get; private set; } = string.Empty;
        public string HotelAddress { get; private set; } = string.Empty;
        public string? HotelPhone { get; private set; }
        public bool IsUrgent { get; private set; }
        public int PaymentMethodId { get; private set; } // PaymentMethod enum
        public OrderState OrderState { get; private set; } = OrderState.Pending;

        // Navigation properties
        public Customer Customer { get; private set; } = null!;
        public SubCategory SubCategory { get; private set; } = null!;
        public City City { get; private set; } = null!;
        public ICollection<OrderVehicle> OrderVehicles { get; private set; } = new List<OrderVehicle>();
        public ICollection<OrderPayment> OrderPayments { get; private set; } = new List<OrderPayment>();
        public ICollection<ReservedVehiclesPerDays> ReservedVehiclesPerDays { get; private set; } = new List<ReservedVehiclesPerDays>();
        public OrderCancellationFee? OrderCancellationFee { get; private set; }

        // Audit properties
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Private constructor for EF Core
        private Order() { }

        // Factory method for creating orders
        public static Order Create(
            int customerId,
            int subCategoryId,
            int cityId,
            DateTime reservationDateFrom,
            DateTime reservationDateTo,
            int vehiclesCount,
            decimal orderSubTotal,
            decimal orderTotal,
            string passportImage,
            string hotelName,
            string hotelAddress,
            int paymentMethodId,
            bool isUrgent,
            string orderCode,
            string? hotelPhone = null,
            string? notes = null,
            string? createdBy = null)
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero", nameof(customerId));

            if (subCategoryId <= 0)
                throw new ArgumentException("SubCategory ID must be greater than zero", nameof(subCategoryId));

            if (cityId <= 0)
                throw new ArgumentException("City ID must be greater than zero", nameof(cityId));

            if (reservationDateFrom >= reservationDateTo)
                throw new ArgumentException("Reservation date from must be before reservation date to", nameof(reservationDateFrom));

            if (vehiclesCount <= 0)
                throw new ArgumentException("Vehicles count must be greater than zero", nameof(vehiclesCount));

            if (orderSubTotal < 0)
                throw new ArgumentException("Order sub total cannot be negative", nameof(orderSubTotal));

            if (orderTotal < 0)
                throw new ArgumentException("Order total cannot be negative", nameof(orderTotal));

            if (string.IsNullOrWhiteSpace(passportImage))
                throw new ArgumentException("Passport image is required", nameof(passportImage));

            if (string.IsNullOrWhiteSpace(hotelName))
                throw new ArgumentException("Hotel name is required", nameof(hotelName));

            if (string.IsNullOrWhiteSpace(hotelAddress))
                throw new ArgumentException("Hotel address is required", nameof(hotelAddress));

            if (string.IsNullOrWhiteSpace(orderCode))
                throw new ArgumentException("Order code is required", nameof(orderCode));

            if (!Enum.IsDefined(typeof(PaymentMethod), paymentMethodId))
                throw new ArgumentException("Invalid payment method", nameof(paymentMethodId));

            return new Order
            {
                OrderCode = orderCode,
                CustomerId = customerId,
                SubCategoryId = subCategoryId,
                CityId = cityId,
                ReservationDateFrom = reservationDateFrom,
                ReservationDateTo = reservationDateTo,
                VehiclesCount = vehiclesCount,
                OrderSubTotal = orderSubTotal,
                OrderTotal = orderTotal,
                PassportImage = passportImage,
                HotelName = hotelName.Trim(),
                HotelAddress = hotelAddress.Trim(),
                HotelPhone = hotelPhone?.Trim(),
                IsUrgent = isUrgent,
                PaymentMethodId = paymentMethodId,
                OrderState = OrderState.Pending,
                Notes = notes,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };
        }

        // Domain methods
        public void Confirm(string? modifiedBy = null)
        {
            if (OrderState != OrderState.Pending)
                throw new InvalidOperationException($"Cannot confirm order in {OrderState} state. Order must be in Pending state.");

            OrderState = OrderState.Confirmed;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void MarkOnWay(string? modifiedBy = null)
        {
            if (OrderState != OrderState.Confirmed)
                throw new InvalidOperationException($"Cannot mark order as OnWay in {OrderState} state. Order must be in Confirmed state.");

            OrderState = OrderState.OnWay;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void MarkCustomerReceived(string? modifiedBy = null)
        {
            if (OrderState != OrderState.OnWay)
                throw new InvalidOperationException($"Cannot mark customer received in {OrderState} state. Order must be in OnWay state.");

            OrderState = OrderState.CustomerReceived;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void Complete(string? modifiedBy = null)
        {
            if (OrderState != OrderState.CustomerReceived)
                throw new InvalidOperationException($"Cannot complete order in {OrderState} state. Order must be in CustomerReceived state.");

            OrderState = OrderState.Completed;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void Cancel(string? modifiedBy = null)
        {
            if (OrderState == OrderState.Completed)
                throw new InvalidOperationException("Cannot cancel a completed order.");

            // Note: We don't change OrderState to a "Cancelled" state
            // Instead, we track cancellation through OrderCancellationFee and ReservedVehiclesPerDays.State
            // The order remains in its current state but is effectively cancelled
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void UpdateNotes(string? notes, string? modifiedBy = null)
        {
            Notes = notes;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }
    }
}

