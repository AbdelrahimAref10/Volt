using Domain.Common;

namespace Domain.Models
{
    public class RefreshToken : IAuditable
    {
        public int RefreshTokenId { get; private set; }
        public int UserId { get; private set; }
        public string Token { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public bool IsRevoked { get; private set; }
        public bool IsUsed { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public string? ReplacedByToken { get; private set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Navigation property
        public ApplicationUser User { get; private set; } = null!;

        private RefreshToken() { } // EF Core

        public static RefreshToken Create(
            int userId,
            string token,
            DateTime expiresAt,
            string createdBy)
        {
            return new RefreshToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = expiresAt,
                IsRevoked = false,
                IsUsed = false,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };
        }

        public void Revoke(string? replacedByToken = null)
        {
            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
            ReplacedByToken = replacedByToken;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void MarkAsUsed()
        {
            IsUsed = true;
            LastModifiedDate = DateTime.UtcNow;
        }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsUsed && !IsExpired;
    }
}

