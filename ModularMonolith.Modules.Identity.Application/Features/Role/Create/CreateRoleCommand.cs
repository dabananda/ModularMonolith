using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.Create
{
    public record CreateRoleCommand(
        string Name,
        string? Description) : IRequest<Result>;
}
