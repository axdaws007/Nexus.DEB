using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class StandardVersionConfiguration : IEntityTypeConfiguration<StandardVersion>
    {
        public void Configure(EntityTypeBuilder<StandardVersion> builder)
        {
            builder.ToTable("StandardVersion", "deb");

            builder.HasBaseType<EntityHead>();

            builder.Property(x => x.Delimiter).HasDefaultValue(string.Empty).HasMaxLength(5);

            builder
                .HasOne<Standard>(x => x.Standard)
                .WithMany()
                .HasForeignKey(x => x.StandardId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasMany(x => x.Requirements)
                .WithMany(y => y.StandardVersions)
                .UsingEntity(z => z.ToTable("StandardVersionRequirement", "deb"));

            builder
                .HasMany(x => x.Requirements)
                .WithMany(y => y.StandardVersions)
                .UsingEntity<Dictionary<string, object>>(
                    "StandardVersionRequirement",
                    // Right side (Requirement)
                    j => j
                        .HasOne<Requirement>()
                        .WithMany()
                        .HasForeignKey("RequirementId")
                        .OnDelete(DeleteBehavior.NoAction),
                    // Left side (StandardVersion)
                    j => j
                        .HasOne<StandardVersion>()
                        .WithMany()
                        .HasForeignKey("StandardVersionId")
                        .OnDelete(DeleteBehavior.NoAction),
                    // Join table configuration
                    j => j.ToTable("StandardVersionRequirement", "deb")
                );
        }
    }
}
