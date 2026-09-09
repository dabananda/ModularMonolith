using ModularMonolith.Modules.Identity.Application.Common;
using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Interfaces;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Auth.ResetPassword
{
    public class ResetPasswordCommandHandler(
        IAuthRepository authRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork) : IRequestHandler<ResetPasswordCommand, Result>
    {
        public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await authRepository.GetByPasswordResetTokenAsync(request.Token, cancellationToken);
            if (user is null)
                return Result.Failure(ErrorType.Unauthorized, AuthErrors.InvalidPasswordResetToken);

            var token = user.PasswordResetTokens.First(t => t.Token == request.Token);

            if (token.IsConsumed)
                return Result.Failure(ErrorType.Unauthorized, AuthErrors.InvalidPasswordResetToken);

            if (token.IsExpired)
                return Result.Failure(ErrorType.Unauthorized, AuthErrors.PasswordResetTokenExpired);

            var newPasswordHash = passwordHasher.HashPassword(request.NewPassword);

            user.ResetPassword(request.Token, newPasswordHash);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}