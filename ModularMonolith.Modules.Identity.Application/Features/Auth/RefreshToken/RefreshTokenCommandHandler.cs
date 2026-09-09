using ModularMonolith.Modules.Identity.Application.Common;
using ModularMonolith.Modules.Identity.Application.Features.Auth.Login;
using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Interfaces;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Auth.RefreshToken
{
    public class RefreshTokenCommandHandler(
        IAuthRepository authRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork) : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
    {
        public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var ip = currentUserService.IpAddress;
            var user = await authRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
            if (user is null)
                return Result<LoginResponse>.Failure(ErrorType.Unauthorized, AuthErrors.InvalidRefreshToken);

            var currentToken = user.RefreshTokens.First(t => t.Token == request.RefreshToken);

            if (currentToken.IsRevoked)
                return Result<LoginResponse>.Failure(ErrorType.Unauthorized, AuthErrors.InvalidRefreshToken);

            if (currentToken.IsExpired)
                return Result<LoginResponse>.Failure(ErrorType.Unauthorized, AuthErrors.RefreshTokenExpired);

            if (!user.IsActive)
                return Result<LoginResponse>.Failure(ErrorType.NotFound, AuthErrors.AccountDeactivated);

            var roles = user.UserRoles.Select(ur => ur.Role.Name);

            var (accessToken, accessTokenExpiresAt) = jwtTokenGenerator.GenerateAccessToken(user, roles);
            var (newRefreshTokenValue, newRefreshTokenExpiresAt) = jwtTokenGenerator.GenerateRefreshToken();

            var newRefreshToken = user.IssueRefreshToken(newRefreshTokenValue, newRefreshTokenExpiresAt, ip);
            user.RevokeRefreshToken(request.RefreshToken, ip, newRefreshTokenValue);

            await authRepository.AddRefreshTokenAsync(newRefreshToken, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new LoginResponse(
                newRefreshToken.User.Id,
                newRefreshToken.User.Email,
                accessToken,
                accessTokenExpiresAt,
                newRefreshToken.Token,
                newRefreshTokenExpiresAt);

            return Result<LoginResponse>.Success(response);
        }
    }
}