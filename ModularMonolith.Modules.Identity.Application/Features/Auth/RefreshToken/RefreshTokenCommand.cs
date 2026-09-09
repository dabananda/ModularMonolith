using ModularMonolith.Modules.Identity.Application.Features.Auth.Login;
using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Auth.RefreshToken
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<LoginResponse>>;
}