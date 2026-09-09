using ModularMonolith.Shared.Entities;

namespace ModularMonolith.Modules.Identity.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; private set; }
        public ApplicationUser User { get; private set; } = null!;
        public string Token { get; private set; } = string.Empty;
        public DateTime ExpireAt { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public string? ReplacedByToken { get; private set; }
        public string? CreatedByIp { get; private set; }
        public string? RevokedByIp { get; private set; }

        public bool IsExpired => DateTime.UtcNow >= ExpireAt;
        public bool IsRevoked => RevokedAt is not null;
        public bool IsActive => !IsRevoked && !IsExpired;

        private RefreshToken() { }

        internal static RefreshToken Create(ApplicationUser user, string token, DateTime expireAt, string? createdByIp)
        {
            if (expireAt <= DateTime.UtcNow)
                throw new ArgumentException("Expiry must be in the future.", nameof(expireAt));

            return new RefreshToken
            {
                UserId = user.Id,
                User = user,
                Token = token,
                ExpireAt = expireAt,
                CreatedByIp = createdByIp
            };
        }

        internal void Revoke(string? revokedByIp, string? replacedByToken)
        {
            if (IsRevoked) return;
            RevokedAt = DateTime.UtcNow;
            RevokedByIp = revokedByIp;
            ReplacedByToken = replacedByToken;
        }
    }
}