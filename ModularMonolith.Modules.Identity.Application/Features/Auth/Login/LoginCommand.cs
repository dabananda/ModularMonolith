using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Auth.Login
{
    public record LoginCommand(
        string Email,
        string Password) : IRequest<Result<LoginResponse>>;
}