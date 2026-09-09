using ModularMonolith.Shared.Entities;

namespace ModularMonolith.Modules.Identity.Domain.Entities
{
    public class ApplicationUser : BaseEntity
    {
        public string Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public string Username { get; private set; } = null!;
        public bool EmailConfirmed { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime? DeactivatedAt { get; private set; }

        private readonly List<UserRole> _userRoles = [];
        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

        private readonly List<RefreshToken> _refreshTokens = [];
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        private readonly List<EmailVerificationToken> _emailVerificationTokens = [];
        public IReadOnlyCollection<EmailVerificationToken> EmailVerificationTokens => _emailVerificationTokens.AsReadOnly();

        private readonly List<PasswordResetToken> _passwordResetTokens = [];
        public IReadOnlyCollection<PasswordResetToken> PasswordResetTokens => _passwordResetTokens.AsReadOnly();

        private ApplicationUser() { }

        public static ApplicationUser Create(string email, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password is required.", nameof(passwordHash));

            return new ApplicationUser
            {
                Email = email.Trim().ToLowerInvariant(),
                Username = email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash
            };
        }

        public void ConfirmEmail()
        {
            if (EmailConfirmed) return;
            EmailConfirmed = true;
        }

        public void RecordLogin()
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot log in. Account is deactivated.");
            LastLoginAt = DateTime.UtcNow;
        }

        public void ChangePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Password is required.", nameof(newPasswordHash));
            PasswordHash = newPasswordHash;
        }

        public void Deactivate()
        {
            if (!IsActive) return;
            IsActive = false;
            DeactivatedAt = DateTime.UtcNow;
        }

        public void Reactivate()
        {
            if (IsActive) return;
            IsActive = true;
            DeactivatedAt = null;
        }

        public void AssignRole(Role role)
        {
            if (_userRoles.Any(ur => ur.RoleId == role.Id)) return;
            _userRoles.Add(UserRole.Create(this, role));
        }

        public void RemoveRole(Guid roleId)
        {
            var link = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
            if (link is null) return;

            _userRoles.Remove(link);
        }

        public RefreshToken IssueRefreshToken(string token, DateTime expireAt, string? createdByIp)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot issue tokens to a deactivated account.");
            var rt = RefreshToken.Create(this, token, expireAt, createdByIp);
            _refreshTokens.Add(rt);
            return rt;
        }

        public void RevokeRefreshToken(string token, string? revokedByIp, string? replacedByToken = null)
        {
            var rt = _refreshTokens.FirstOrDefault(t => t.Token == token)
                ?? throw new InvalidOperationException("Refresh token not found.");
            rt.Revoke(revokedByIp, replacedByToken);
        }

        public EmailVerificationToken IssueEmailVerificationToken(string token, DateTime expireAt)
        {
            if (EmailConfirmed)
                throw new InvalidOperationException("Email is already confirmed.");

            var evt = EmailVerificationToken.Create(this, token, expireAt);
            _emailVerificationTokens.Add(evt);
            return evt;
        }

        public void ConfirmEmailWithToken(string token)
        {
            var evt = _emailVerificationTokens.FirstOrDefault(t => t.Token == token)
                ?? throw new InvalidOperationException("Verification token not found.");

            if (!evt.IsActive)
                throw new InvalidOperationException("Verification token is expired or already used.");

            evt.Consume();
            ConfirmEmail();
        }

        public PasswordResetToken IssuePasswordResetToken(string token, DateTime expireAt, string? createdByIp)
        {
            var prt = PasswordResetToken.Create(this, token, expireAt, createdByIp);
            _passwordResetTokens.Add(prt);
            return prt;
        }

        public void ResetPassword(string token, string newPasswordHash)
        {
            var prt = _passwordResetTokens.FirstOrDefault(t => t.Token == token)
                ?? throw new InvalidOperationException("Password reset token not found.");

            if (!prt.IsActive)
                throw new InvalidOperationException("Password reset token is expired or already used.");

            prt.Consume();
            ChangePassword(newPasswordHash);

            // force re-login everywhere after a password reset
            foreach (var rt in _refreshTokens.Where(t => t.IsActive))
                rt.Revoke(revokedByIp: null, replacedByToken: null);
        }
    }
}