using ModularMonolith.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace ModularMonolith.Shared.Persistence
{
    public class SharedDbContext(DbContextOptions<SharedDbContext> options) : DbContext(options)
    {
        public DbSet<Image> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("shared");
        }
    }
}
