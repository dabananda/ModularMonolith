using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Modules.Identity.Domain.Entities;
using ModularMonolith.Shared.Configurations;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ModularMonolith.Modules.Identity.Infrastructure.Security
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly Settings _settings;
        private readonly SigningCredentials _signingCredentials;
        private readonly JwtSecurityTokenHandler _tokenHandler = new();

        public JwtTokenGenerator(Settings settings)
        {
            _settings = settings;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Jwt.Key));
            _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
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

            var token = new JwtSecurityToken(
                issuer: _settings.Jwt.Issuer,
                audience: _settings.Jwt.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: _signingCredentials);

            var tokenValue = _tokenHandler.WriteToken(token);

            return (tokenValue, expiresAt);
        }

        public (string Token, DateTime ExpiresAt) GenerateRefreshToken()
        {
            Span<byte> randomBytes = stackalloc byte[64];
            RandomNumberGenerator.Fill(randomBytes);
            var token = Convert.ToBase64String(randomBytes);
            var expiresAt = DateTime.UtcNow.AddDays(_settings.Jwt.RefreshTokenExpiryDays);

            return (token, expiresAt);
        }
    }
}