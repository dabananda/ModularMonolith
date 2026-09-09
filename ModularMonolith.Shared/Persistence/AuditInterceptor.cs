using ModularMonolith.Shared.Entities;
using ModularMonolith.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ModularMonolith.Shared.Persistence
{
    public sealed class AuditSaveChangesInterceptor(ICurrentUserService currentUserService) : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            ApplyAuditInformation(eventData.Context);

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            ApplyAuditInformation(eventData.Context);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ApplyAuditInformation(DbContext? context)
        {
            if (context is null)
                return;

            var now = DateTime.UtcNow;
            var userId = currentUserService.UserId;

            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.CreatedBy = userId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.UpdatedBy = userId;

                        HandleSoftDelete(entry, now, userId);
                        break;
                }
            }
        }

        private static void HandleSoftDelete(EntityEntry<BaseEntity> entry, DateTime now, Guid? userId)
        {
            var isDeletedProperty = entry.Property(nameof(BaseEntity.IsDeleted));

            if (isDeletedProperty.IsModified && isDeletedProperty.CurrentValue is true && isDeletedProperty.OriginalValue is not true)
            {
                entry.Entity.DeletedAt = now;
                entry.Entity.DeletedBy = userId;
            }
        }
    }
}