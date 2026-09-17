using ModularMonolith.Modules.Identity.Application.Interfaces;
using System.Buffers.Text;
using System.Security.Cryptography;

namespace ModularMonolith.Modules.Identity.Infrastructure.Security
{
    public class SecureTokenGenerator : ISecureTokenGenerator
    {
        public string GenerateToken()
        {
            Span<byte> bytes = stackalloc byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Base64Url.EncodeToString(bytes);
        }
    }
}
