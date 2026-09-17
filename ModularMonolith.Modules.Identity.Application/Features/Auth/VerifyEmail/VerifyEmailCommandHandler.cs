using ModularMonolith.Modules.Identity.Application.Common;
using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Interfaces;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Auth.VerifyEmail
{
    public class VerifyEmailCommandHandler(
        IAuthRepository authRepository) : IRequestHandler<VerifyEmailCommand, Result>
    {
        public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await authRepository.GetByEmailVerificationTokenAsync(request.Token, cancellationToken);
            if (user is null)
                return Result.Failure(ErrorType.Unauthorized, AuthErrors.InvalidVerificationToken);

            var token = user.EmailVerificationTokens.First(t => t.Token == request.Token);

            if (token.IsConsumed)
                return Result.Failure(ErrorType.Unauthorized, AuthErrors.InvalidVerificationToken);

            if (token.IsExpired)
                return Result.Failure(ErrorType.Unauthorized, AuthErrors.VerificationTokenExpired);

            user.ConfirmEmailWithToken(request.Token);

            await authRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}