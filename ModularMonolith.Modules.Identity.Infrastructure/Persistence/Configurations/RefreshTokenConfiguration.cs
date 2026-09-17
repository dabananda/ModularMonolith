using ModularMonolith.Modules.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ModularMonolith.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Token)
                   .HasMaxLength(EntityConstraints.TokenMaxLength)
                   .IsRequired();

            builder.HasIndex(x => x.Token)
                   .IsUnique();

            builder.HasIndex(x => x.UserId);
        }
    }
}
