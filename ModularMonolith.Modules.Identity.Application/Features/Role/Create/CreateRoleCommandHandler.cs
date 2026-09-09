using ModularMonolith.Modules.Identity.Application.Common;
using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Interfaces;
using ModularMonolith.Shared.Messaging;
using RoleEntity = ModularMonolith.Modules.Identity.Domain.Entities.Role;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.Create
{
    public class CreateRoleCommandHandler(
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateRoleCommand, Result>
    {
        public async Task<Result> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            if (await roleRepository.ExistsByNameAsync(request.Name, cancellationToken))
                return Result.Failure(ErrorType.Conflict, RoleErrors.AlreadyExists);

            var role = RoleEntity.Create(request.Name, request.Description);

            await roleRepository.AddRoleAsync(role, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}