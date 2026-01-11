using Domain.Common;
using Domain.Enums;

namespace Domain.Models
{
    public class Customer : IAuditable
    {
        public int CustomerId { get; private set; }
        public string MobileNumber { get; private set; } = string.Empty;
        public string FullName { get; private set; } = string.Empty;
        public string Gender { get; private set; } = string.Empty; // "Male", "Female", etc.
        public string? PersonalImage { get; private set; }
        public string? FullAddress { get; private set; }
        public int RegisterAs { get; private set; } // 0 = Individual, 1 = Institution
        public int VerificationBy { get; private set; } // 0 = Phone, 1 = Email
        public CustomerState State { get; private set; } = CustomerState.InActive;
        public bool CashBlock { get; private set; } = false;
        public string? InvitationCode { get; private set; }
        public DateTime? InvitationCodeExpiry { get; private set; }
        public bool IsInvitationCodeUsed { get; private set; } = false;
        public string PasswordHash { get; private set; } = string.Empty;

        // Navigation property to City
        public int CityId { get; private set; }
        public City City { get; private set; } = null!;

        // Audit properties
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Factory method for creating customers
        public static Customer Create(
            string mobileNumber,
            string fullName,
            string gender,
            string invitationCode,
            string passwordHash,
            int cityId,
            int registerAs,
            int verificationBy,
            string? fullAddress = null,
            string? personalImage = null,
            string? createdBy = null)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
                throw new ArgumentException("Mobile number cannot be empty", nameof(mobileNumber));

            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name cannot be empty", nameof(fullName));

            if (string.IsNullOrWhiteSpace(gender))
                throw new ArgumentException("Gender cannot be empty", nameof(gender));

            if (string.IsNullOrWhiteSpace(invitationCode))
                throw new ArgumentException("Invitation code cannot be empty", nameof(invitationCode));

            if (!Enum.IsDefined(typeof(RegisterAs), registerAs))
                throw new ArgumentException("Invalid RegisterAs value", nameof(registerAs));

            if (!Enum.IsDefined(typeof(VerificationBy), verificationBy))
                throw new ArgumentException("Invalid VerificationBy value", nameof(verificationBy));

            return new Customer
            {
                MobileNumber = mobileNumber,
                FullName = fullName,
                Gender = gender,
                PersonalImage = personalImage,
                FullAddress = fullAddress,
                RegisterAs = registerAs,
                VerificationBy = verificationBy,
                State = CustomerState.InActive,
                InvitationCode = invitationCode,
                InvitationCodeExpiry = DateTime.UtcNow.AddHours(24),
                IsInvitationCodeUsed = false,
                PasswordHash = passwordHash,
                CityId = cityId,
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
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        // Activate without clearing invitation code (for admin-created customers)
        public void ActivateWithoutClearingCode(string? modifiedBy = null)
        {
            State = CustomerState.Active;
            IsInvitationCodeUsed = true;
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

        public void Deactivate(string? modifiedBy = null)
        {
            State = CustomerState.InActive;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void BlockCashPayment(string? modifiedBy = null)
        {
            CashBlock = true;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void UnblockCashPayment(string? modifiedBy = null)
        {
            CashBlock = false;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
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

        public void UpdateProfile(string fullName, string gender, int cityId, string? fullAddress = null, string? personalImage = null, string? modifiedBy = null)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name cannot be empty", nameof(fullName));

            if (string.IsNullOrWhiteSpace(gender))
                throw new ArgumentException("Gender cannot be empty", nameof(gender));

            FullName = fullName;
            Gender = gender;
            CityId = cityId;
            FullAddress = fullAddress;
            PersonalImage = personalImage;
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }
    }
}

