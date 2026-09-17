using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Interfaces;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Auth.Logout
{
    public class LogoutCommandHandler(
        IAuthRepository authRepository,
        ICurrentUserService currentUserService) : IRequestHandler<LogoutCommand, Result>
    {
        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var ip = currentUserService.IpAddress;
            var user = await authRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
            if (user is null)
                return Result.Success();

            var token = user.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);
            if (token is null || token.IsRevoked)
                return Result.Success();

            user.RevokeRefreshToken(request.RefreshToken, ip);

            await authRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}