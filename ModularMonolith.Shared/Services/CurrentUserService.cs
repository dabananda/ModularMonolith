using Microsoft.AspNetCore.Http;
using ModularMonolith.Shared.Interfaces;
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
                var user = httpContextAccessor.HttpContext?.User;
                var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? user?.FindFirstValue("sub");
                return Guid.TryParse(userId, out var id) ? id : null;
            }
        }

        public string? IpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }
}
