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
            var normalizedEmail = email.Trim().ToLowerInvariant();
            return await context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        }

        public async Task RegisterAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            if (context.Entry(user).State == EntityState.Detached)
            {
                await context.Users.AddAsync(user, cancellationToken);
            }
        }

        public async Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public void Update(ApplicationUser user)
        {
            if (context.Entry(user).State == EntityState.Detached)
            {
                context.Users.Update(user);
            }
        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();

            return await context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        }

        public async Task<ApplicationUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return await context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.RefreshTokens.Where(rt => rt.Token == refreshToken))
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken), cancellationToken);
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            if (context.Entry(refreshToken).State == EntityState.Detached)
            {
                await context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
            }
        }

        public async Task AddEmailVerificationTokenAsync(EmailVerificationToken token, CancellationToken cancellationToken = default)
        {
            if (context.Entry(token).State == EntityState.Detached)
            {
                await context.EmailVerificationTokens.AddAsync(token, cancellationToken);
            }
        }

        public async Task<ApplicationUser?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await context.Users
                .Include(u => u.EmailVerificationTokens.Where(t => t.Token == token))
                .FirstOrDefaultAsync(u => u.EmailVerificationTokens.Any(t => t.Token == token), cancellationToken);
        }

        public async Task AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
        {
            if (context.Entry(token).State == EntityState.Detached)
            {
                await context.PasswordResetTokens.AddAsync(token, cancellationToken);
            }
        }

        public async Task<ApplicationUser?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await context.Users
                .Include(u => u.PasswordResetTokens.Where(t => t.Token == token))
                .Include(u => u.RefreshTokens.Where(rt => rt.RevokedAt == null && rt.ExpireAt > DateTime.UtcNow))
                .FirstOrDefaultAsync(u => u.PasswordResetTokens.Any(t => t.Token == token), cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
    }
}