using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.Assign
{
    public record AssignRoleCommand(Guid UserId, Guid RoleId) : IRequest<Result>;
}
