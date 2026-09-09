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
    public class JwtTokenGenerator(Settings settings) : IJwtTokenGenerator
    {
        public (string Token, DateTime ExpiresAt) GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(settings.Jwt.AccessTokenExpiryMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("username", user.Username)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Jwt.Key));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: settings.Jwt.Issuer,
                audience: settings.Jwt.Audience,
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
            var expiresAt = DateTime.UtcNow.AddDays(settings.Jwt.RefreshTokenExpiryDays);

            return (token, expiresAt);
        }
    }
}