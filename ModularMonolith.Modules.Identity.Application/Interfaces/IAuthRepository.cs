using ModularMonolith.Modules.Identity.Domain.Entities;

namespace ModularMonolith.Modules.Identity.Application.Interfaces
{
    public interface IAuthRepository
    {
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
        Task RegisterAsync(ApplicationUser user, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
        Task AddEmailVerificationTokenAsync(EmailVerificationToken token, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default);
        Task AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
        void Update(ApplicationUser user);
    }
}