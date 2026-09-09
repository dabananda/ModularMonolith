using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Auth.Register
{
    public record RegisterCommand(
        string Email,
        string Password) : IRequest<Result>;
}