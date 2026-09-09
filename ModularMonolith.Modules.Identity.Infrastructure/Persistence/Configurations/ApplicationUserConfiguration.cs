using ModularMonolith.Modules.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ModularMonolith.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                .HasMaxLength(EntityConstraints.EmailMaxLength)
                .IsRequired();

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.Property(x => x.Username)
                .HasMaxLength(EntityConstraints.UserNameMaxLength)
                .IsRequired();

            builder.HasIndex(x => x.Username)
                .IsUnique();

            builder.Property(x => x.PasswordHash)
                .HasMaxLength(EntityConstraints.PasswordHashMaxLength)
                .IsRequired();
        }
    }
}
