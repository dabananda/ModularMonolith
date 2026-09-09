using ModularMonolith.Modules.Identity.Application.Common;
using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Interfaces;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.Delete
{
    public class DeleteRoleCommandHandler(
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteRoleCommand, Result>
    {
        public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken);

            if (role is null)
                return Result.Failure(ErrorType.NotFound, RoleErrors.NotFound);

            role.MarkDelete();

            roleRepository.UpdateRole(role);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}