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
            builder.ToTable("StandardVersion", "deb", t => { t.HasTrigger("StandardVersion_ChangeTracking"); });

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
                .UsingEntity<StandardVersionRequirement>(
                    j => j
                        .HasOne<Requirement>()
                        .WithMany()
                        .HasForeignKey(x => x.RequirementId)
                        .OnDelete(DeleteBehavior.NoAction),
                    j => j
                        .HasOne<StandardVersion>()
                        .WithMany()
                        .HasForeignKey(x => x.StandardVersionId)
                        .OnDelete(DeleteBehavior.NoAction)
                );

            builder.Property(x => x.VersionTitle).HasDefaultValue(string.Empty).HasMaxLength(50);
        }
    }
}
