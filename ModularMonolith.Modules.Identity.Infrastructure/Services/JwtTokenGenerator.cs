using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Modules.Identity.Domain.Entities;
using ModularMonolith.Shared.Configurations;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ModularMonolith.Modules.Identity.Infrastructure.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly Settings _settings;
        private readonly RsaSecurityKey _signingKey;

        public JwtTokenGenerator(Settings settings, IHostEnvironment environment)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrWhiteSpace(_settings.Jwt.Key))
                throw new InvalidOperationException("Jwt:PrivateKeyPath is missing.");

            var privateKeyFullPath = Path.Combine(environment.ContentRootPath, _settings.Jwt.Key);

            if (!File.Exists(privateKeyFullPath))
                throw new InvalidOperationException($"Jwt private key file not found at '{privateKeyFullPath}'.");

            var privateKeyPem = File.ReadAllText(privateKeyFullPath);

            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem);

            // Cache the key/RSA instance for the lifetime of this generator instead of
            // re-parsing the PEM on every token issuance.
            _signingKey = new RsaSecurityKey(rsa);
        }

        public (string Token, DateTime ExpiresAt) GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_settings.Jwt.AccessTokenExpiryMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("username", user.Username)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.RsaSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Jwt.Issuer,
                audience: _settings.Jwt.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: signingCredentials);

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

            return (tokenValue, expiresAt);
        }

        public (string Token, DateTime ExpiresAt) GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(randomBytes);
            var expiresAt = DateTime.UtcNow.AddDays(_settings.Jwt.RefreshTokenExpiryDays);

            return (token, expiresAt);
        }
    }
}
