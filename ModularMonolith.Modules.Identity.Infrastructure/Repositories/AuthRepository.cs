using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Modules.Identity.Domain.Entities;
using ModularMonolith.Modules.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ModularMonolith.Modules.Identity.Infrastructure.Repositories
{
    public class AuthRepository(ApplicationDbContext context) : IAuthRepository
    {
        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return await context.Users.AnyAsync(u => u.Email == email, cancellationToken);
        }

        public async Task RegisterAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            await context.Users.AddAsync(user, cancellationToken);
        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower(), cancellationToken);
        }

        public async Task<ApplicationUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return await context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken), cancellationToken);
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            await context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        }

        public async Task AddEmailVerificationTokenAsync(EmailVerificationToken token, CancellationToken cancellationToken = default)
        {
            await context.EmailVerificationTokens.AddAsync(token, cancellationToken);
        }

        public async Task<ApplicationUser?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await context.Users
                .Include(u => u.EmailVerificationTokens)
                .FirstOrDefaultAsync(u => u.EmailVerificationTokens.Any(t => t.Token == token), cancellationToken);
        }

        public async Task AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
        {
            await context.PasswordResetTokens.AddAsync(token, cancellationToken);
        }

        public async Task<ApplicationUser?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await context.Users
                .Include(u => u.PasswordResetTokens)
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.PasswordResetTokens.Any(t => t.Token == token), cancellationToken);
        }
    }
}