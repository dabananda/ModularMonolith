using ModularMonolith.Modules.Identity.Application.Common;
using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Interfaces;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Auth.Login
{
    public class LoginCommandHandler(
        IAuthRepository authRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ICurrentUserService currentUserService) : IRequestHandler<LoginCommand, Result<LoginResponse>>
    {
        public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var ip = currentUserService.IpAddress;
            var user = await authRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
                return Result<LoginResponse>.Failure(ErrorType.Unauthorized, AuthErrors.InvalidCredentials);

            if (!user.EmailConfirmed)
                return Result<LoginResponse>.Failure(ErrorType.Forbidden, AuthErrors.EmailNotConfirmed);

            if (!user.IsActive)
                return Result<LoginResponse>.Failure(ErrorType.NotFound, AuthErrors.AccountDeactivated);

            var roles = user.UserRoles.Select(ur => ur.Role.Name);

            var (accessToken, accessTokenExpiresAt) = jwtTokenGenerator.GenerateAccessToken(user, roles);
            var (refreshTokenValue, refreshTokenExpiresAt) = jwtTokenGenerator.GenerateRefreshToken();

            var refreshToken = user.IssueRefreshToken(refreshTokenValue, refreshTokenExpiresAt, ip);
            user.RecordLogin();

            await authRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
            await authRepository.SaveChangesAsync(cancellationToken);

            var response = new LoginResponse(refreshToken.User.Id, refreshToken.User.Email, accessToken, accessTokenExpiresAt, refreshToken.Token, refreshTokenExpiresAt);
            return Result<LoginResponse>.Success(response);
        }
    }
}