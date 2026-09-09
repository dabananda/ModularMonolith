namespace ModularMonolith.Modules.Identity.Application.Common
{
    public static class AuthErrors
    {
        public const string InvalidCredentials = "Invalid email or password.";
        public const string EmailAlreadyExists = "Email already exists.";
        public const string EmailNotConfirmed = "Email not confirmed.";
        public const string AccountDeactivated = "This account has been deactivated.";
        public const string InvalidRefreshToken = "Invalid refresh token.";
        public const string RefreshTokenExpired = "Refresh token has expired or been revoked.";
        public const string InvalidVerificationToken = "Invalid or already used email verification token.";
        public const string VerificationTokenExpired = "Email verification token has expired.";
        public const string InvalidPasswordResetToken = "Invalid or already used password reset token.";
        public const string PasswordResetTokenExpired = "Password reset token has expired.";
    }
}