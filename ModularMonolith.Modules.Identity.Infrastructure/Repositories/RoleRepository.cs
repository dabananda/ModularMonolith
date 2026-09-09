using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Modules.Identity.Domain.Entities;
using ModularMonolith.Modules.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ModularMonolith.Modules.Identity.Infrastructure.Repositories
{
    public class RoleRepository(ApplicationDbContext context) : IRoleRepository
    {
        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await context.Roles.AnyAsync(r => r.Name == name, cancellationToken);
        }

        public async Task AddRoleAsync(Role role, CancellationToken cancellationToken = default)
        {
            await context.Roles.AddAsync(role, cancellationToken);
        }

        public async Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await context.Roles.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
        }

        public async Task<bool> ExistsByNameAsync(string name, Guid excludingId, CancellationToken cancellationToken = default)
        {
            return await context.Roles.AnyAsync(r => r.Name == name && r.Id != excludingId, cancellationToken);
        }

        public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await context.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await context.Roles.OrderBy(r => r.Name).ToListAsync(cancellationToken);
        }

        public void UpdateRole(Role role)
        {
            context.Roles.Update(role);
        }
    }
}