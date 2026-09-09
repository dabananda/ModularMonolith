using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Shared.Configurations
{
    public sealed class Settings
    {
        public ConnectionStrings ConnectionStrings { get; set; } = new();
        public CorsSettings Cors { get; set; } = new();
        public CloudinarySettings Cloudinary { get; set; } = new();
        public JwtSettings Jwt { get; set; } = new();
        public EmailSettings Email { get; set; } = new();
    }

    public sealed class ConnectionStrings
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "SqlServerLocal connection string is required.")]
        public string SqlServerLocal { get; set; } = string.Empty;

        public string SqlServerLive { get; set; } = string.Empty;
        public string PostgresLive { get; set; } = string.Empty;
    }

    public sealed class CorsSettings
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "CORS PolicyName is required.")]
        public string PolicyName { get; set; } = string.Empty;

        public string[] AllowedOrigins { get; set; } = [];
    }

    public sealed class CloudinarySettings
    {
        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
    }

    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenExpiryMinutes { get; set; } = 15;
        public int RefreshTokenExpiryDays { get; set; } = 7;
    }

    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://localhost:5000";
        public string ClientUrl { get; set; } = "https://localhost:3000";
    }
}
