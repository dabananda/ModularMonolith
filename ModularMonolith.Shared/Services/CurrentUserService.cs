using ModularMonolith.Shared.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ModularMonolith.Shared.Services
{
    public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

        public Guid? UserId
        {
            get
            {
                var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(userId, out var id) ? id : null;
            }
        }

        public string? IpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }
}
