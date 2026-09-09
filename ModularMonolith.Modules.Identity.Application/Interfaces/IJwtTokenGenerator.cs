using ModularMonolith.Modules.Identity.Domain.Entities;

namespace ModularMonolith.Modules.Identity.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        (string Token, DateTime ExpiresAt) GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles);
        (string Token, DateTime ExpiresAt) GenerateRefreshToken();
    }
}
