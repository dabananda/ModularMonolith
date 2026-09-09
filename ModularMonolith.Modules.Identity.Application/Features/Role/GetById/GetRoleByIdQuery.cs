using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.GetById
{
    public record GetRoleByIdQuery(Guid Id) : IRequest<Result<RoleResponse>>;
}