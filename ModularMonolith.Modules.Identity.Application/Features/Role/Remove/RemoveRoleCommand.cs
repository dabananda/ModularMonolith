using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.Remove
{
    public record RemoveRoleCommand(Guid UserId, Guid RoleId) : IRequest<Result>;
}
