using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Auth.Logout
{
    public record LogoutCommand(string RefreshToken) : IRequest<Result>;
}