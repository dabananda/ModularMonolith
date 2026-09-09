using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.Update
{
    public record UpdateRoleCommand(
        Guid Id,
        string Name,
        string? Description) : IRequest<Result>;
}