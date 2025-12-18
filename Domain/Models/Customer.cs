using Domain.Common;
using Domain.Enums;

namespace Domain.Models
{
    public class Customer : IAuditable
    {
        // Private setters for encapsulation
        public int CustomerId { get; private set; }
        public string MobileNumber { get; private set; } = string.Empty;
        public string UserName { get; private set; } = string.Empty;
        public string NationalNumber { get; private set; } = string.Empty;
        public string Gender { get; private set; } = string.Empty; // "Male", "Female", etc.
        public CustomerState State { get; private set; } = CustomerState.InActive;
        public string? InvitationCode { get; private set; }
        public DateTime? InvitationCodeExpiry { get; private set; }
        public bool IsInvitationCodeUsed { get; private set; } = false;

        // Navigation property to ApplicationUser
        public int UserId { get; private set; }
        public ApplicationUser? User { get; private set; }

        // Audit properties
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Factory method for creating customers
        public static Customer Create(
            string mobileNumber,
            string userName,
            string nationalNumber,
            string gender,
            string invitationCode,
            int userId,
            string? createdBy = null)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
                throw new ArgumentException("Mobile number cannot be empty", nameof(mobileNumber));

            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("User name cannot be empty", nameof(userName));

            if (string.IsNullOrWhiteSpace(nationalNumber))
                throw new ArgumentException("National number cannot be empty", nameof(nationalNumber));

            if (string.IsNullOrWhiteSpace(gender))
                throw new ArgumentException("Gender cannot be empty", nameof(gender));

            if (string.IsNullOrWhiteSpace(invitationCode))
                throw new ArgumentException("Invitation code cannot be empty", nameof(invitationCode));

            return new Customer
            {
                MobileNumber = mobileNumber,
                UserName = userName,
                NationalNumber = nationalNumber,
                Gender = gender,
                State = CustomerState.InActive, // New customers start as InActive
                InvitationCode = invitationCode,
                InvitationCodeExpiry = DateTime.UtcNow.AddHours(24), // Code expires in 24 hours
                IsInvitationCodeUsed = false,
                UserId = userId,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };
        }

        // Domain methods
        public void Activate(string? modifiedBy = null)
        {
            State = CustomerState.Active;
            IsInvitationCodeUsed = true;
            InvitationCode = null; // Clear the code after use
            InvitationCodeExpiry = null;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void Block(string? modifiedBy = null)
        {
            State = CustomerState.Blocked;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void Unblock(string? modifiedBy = null)
        {
            if (State == CustomerState.Blocked)
            {
                State = CustomerState.Active;
                LastModifiedBy = modifiedBy;
                LastModifiedDate = DateTime.UtcNow;
            }
        }

        public bool ValidateInvitationCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            if (IsInvitationCodeUsed)
                return false;

            if (InvitationCodeExpiry.HasValue && InvitationCodeExpiry.Value < DateTime.UtcNow)
                return false;

            return InvitationCode == code;
        }

        public void UpdateProfile(string userName, string nationalNumber, string gender, string? modifiedBy = null)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("User name cannot be empty", nameof(userName));

            if (string.IsNullOrWhiteSpace(nationalNumber))
                throw new ArgumentException("National number cannot be empty", nameof(nationalNumber));

            if (string.IsNullOrWhiteSpace(gender))
                throw new ArgumentException("Gender cannot be empty", nameof(gender));

            UserName = userName;
            NationalNumber = nationalNumber;
            Gender = gender;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }
    }
}

