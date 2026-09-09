using ModularMonolith.Modules.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ModularMonolith.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(EntityConstraints.RoleNameMaxLength)
                .IsRequired();

            builder.HasIndex(x => x.Name)
                .IsUnique();
        }
    }
}
