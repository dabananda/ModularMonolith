using ModularMonolith.Modules.Identity.Application.Interfaces;
using System.Security.Cryptography;

namespace ModularMonolith.Modules.Identity.Infrastructure.Security
{
    internal class SecureTokenGenerator : ISecureTokenGenerator
    {
        public string GenerateToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}
