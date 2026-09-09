using ModularMonolith.Shared.Entities;

namespace ModularMonolith.Modules.Identity.Domain.Entities
{
    public class PasswordResetToken : BaseEntity
    {
        public Guid UserId { get; private set; }
        public ApplicationUser User { get; private set; } = null!;
        public string Token { get; private set; } = string.Empty;
        public DateTime ExpireAt { get; private set; }
        public DateTime? ConsumedAt { get; private set; }
        public string? CreatedByIp { get; private set; }

        public bool IsExpired => DateTime.UtcNow >= ExpireAt;
        public bool IsConsumed => ConsumedAt is not null;
        public bool IsActive => !IsConsumed && !IsExpired;

        private PasswordResetToken() { }

        internal static PasswordResetToken Create(ApplicationUser user, string token, DateTime expireAt, string? createdByIp)
        {
            if (expireAt <= DateTime.UtcNow)
                throw new ArgumentException("Expiry must be in the future.", nameof(expireAt));

            return new PasswordResetToken
            {
                UserId = user.Id,
                User = user,
                Token = token,
                ExpireAt = expireAt,
                CreatedByIp = createdByIp
            };
        }

        internal void Consume()
        {
            if (IsConsumed) return;
            ConsumedAt = DateTime.UtcNow;
        }
    }
}