using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class ScopeConfiguration : IEntityTypeConfiguration<Scope>
    {
        public void Configure(EntityTypeBuilder<Scope> builder)
        {
            builder.ToTable("Scope", "deb", t => { t.HasTrigger("Scope_ChangeTracking"); });

            builder.HasBaseType<EntityHead>();

            builder
                .HasMany(x => x.Requirements)
                .WithMany(x => x.Scopes)
                .UsingEntity<Dictionary<string, object>>(
                    "ScopeRequirement",
                    // Right side (Requirement)
                    j => j
                        .HasOne<Requirement>()
                        .WithMany()
                        .HasForeignKey("RequirementId")
                        .OnDelete(DeleteBehavior.NoAction),
                    // Left side (Scope)
                    j => j
                        .HasOne<Scope>()
                        .WithMany()
                        .HasForeignKey("ScopeId")
                        .OnDelete(DeleteBehavior.NoAction),
                    // Join table configuration
                    j => j.ToTable("ScopeRequirement", "deb", t => { t.HasTrigger("ScopeRequirement_ChangeTracking"); })
                );

        }
    }
}
