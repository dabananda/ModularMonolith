using ModularMonolith.Modules.Identity.Application.Common;
using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.GetById
{
    public class GetRoleByIdQueryHandler(IRoleRepository roleRepository) : IRequestHandler<GetRoleByIdQuery, Result<RoleResponse>>
    {
        public async Task<Result<RoleResponse>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken);

            if (role is null)
                return Result<RoleResponse>.Failure(ErrorType.NotFound, RoleErrors.NotFound);

            var response = new RoleResponse(role.Id, role.Name, role.Description);

            return Result<RoleResponse>.Success(response);
        }
    }
}