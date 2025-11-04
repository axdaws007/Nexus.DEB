using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class StatementConfiguration : IEntityTypeConfiguration<Statement>
    {
        public void Configure(EntityTypeBuilder<Statement> builder)
        {
            builder.ToTable("Statement", "deb");

            builder.HasBaseType<EntityHead>();

            builder.Property(x => x.Id).HasColumnName("Id");

            builder
                .HasOne<Scope>(x => x.Scope)
                .WithMany()
                .HasForeignKey(x => x.ScopeID)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasMany<Requirement>(x => x.Requirements)
                .WithMany(x => x.Statements)
                .UsingEntity(x => x.ToTable("StatementRequirement", "deb"));

            builder
                .HasMany(x => x.Requirements)
                .WithMany(y => y.Statements)
                .UsingEntity<Dictionary<string, object>>(
                    "StatementRequirement",
                    // Right side (Requirement)
                    j => j
                        .HasOne<Requirement>()
                        .WithMany()
                        .HasForeignKey("RequirementId")
                        .OnDelete(DeleteBehavior.NoAction),
                    // Left side (Statement)
                    j => j
                        .HasOne<Statement>()
                        .WithMany()
                        .HasForeignKey("StatementId")
                        .OnDelete(DeleteBehavior.NoAction),
                    // Join table configuration
                    j => j.ToTable("StatementRequirement", "deb")
                );

            builder
                .HasMany<Domain.Models.Task>(x => x.Tasks)
                .WithOne(x => x.Statement)
                .HasForeignKey(x => x.StatementId);

        }
    }
}