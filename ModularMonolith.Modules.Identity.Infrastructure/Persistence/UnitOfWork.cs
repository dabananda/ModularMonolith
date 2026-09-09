using ModularMonolith.Shared.Interfaces;

namespace ModularMonolith.Modules.Identity.Infrastructure.Persistence
{
    public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
    {
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
    }
}