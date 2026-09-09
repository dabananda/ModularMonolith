using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.Delete
{
    public record DeleteRoleCommand(Guid Id) : IRequest<Result>;
}