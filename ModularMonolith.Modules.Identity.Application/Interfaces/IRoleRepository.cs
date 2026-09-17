using ModularMonolith.Modules.Identity.Domain.Entities;

namespace ModularMonolith.Modules.Identity.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(string name, Guid excludingId, CancellationToken cancellationToken = default);
        Task AddRoleAsync(Role role, CancellationToken cancellationToken = default);
        Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default);
        void UpdateRole(Role role);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}