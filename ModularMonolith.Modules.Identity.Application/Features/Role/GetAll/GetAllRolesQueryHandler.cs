using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.GetAll
{
    public class GetAllRolesQueryHandler(IRoleRepository roleRepository) : IRequestHandler<GetAllRolesQuery, Result<IEnumerable<RoleResponse>>>
    {
        public async Task<Result<IEnumerable<RoleResponse>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await roleRepository.GetAllAsync(cancellationToken);

            var response = roles.Select(r => new RoleResponse(r.Id, r.Name, r.Description));

            return Result<IEnumerable<RoleResponse>>.Success(response);
        }
    }
}