using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.GetAll
{
    public record GetAllRolesQuery() : IRequest<Result<IEnumerable<RoleResponse>>>;
}