using ModularMonolith.Modules.Identity.Application.Common;
using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Interfaces;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.Update
{
    public class UpdateRoleCommandHandler(
        IRoleRepository roleRepository) : IRequestHandler<UpdateRoleCommand, Result>
    {
        public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken);

            if (role is null)
                return Result.Failure(ErrorType.NotFound, RoleErrors.NotFound);

            if (await roleRepository.ExistsByNameAsync(request.Name, request.Id, cancellationToken))
                return Result.Failure(ErrorType.Conflict, RoleErrors.AlreadyExists);

            role.UpdateDetails(request.Name, request.Description);

            await roleRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}