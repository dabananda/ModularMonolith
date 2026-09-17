using ModularMonolith.Modules.Identity.Application.Common;
using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Interfaces;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Role.Remove
{
    public class RemoveRoleCommandHandler(
        IAuthRepository authRepository,
        IRoleRepository roleRepository) : IRequestHandler<RemoveRoleCommand, Result>
    {
        public async Task<Result> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await authRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return Result.Failure(ErrorType.NotFound, "User not found.");
            }

            var role = await roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
            if (role is null)
            {
                return Result.Failure(ErrorType.NotFound, RoleErrors.NotFound);
            }

            user.RemoveRole(request.RoleId);
            authRepository.Update(user);

            await authRepository.SaveChangesAsync(cancellationToken);

            return Result.Success("Role removed successfully.");
        }
    }
}
