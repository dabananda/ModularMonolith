namespace ModularMonolith.Modules.Identity.Application.Features.Auth.Login
{
    public record LoginResponse(
        Guid UserId,
        string Email,
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt);
}