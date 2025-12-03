using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class StatementRequirementScopeConfiguration : IEntityTypeConfiguration<StatementRequirementScope>
    {
        public void Configure(EntityTypeBuilder<StatementRequirementScope> builder)
        {
            builder.ToTable("StatementRequirementScope", "deb", t => { t.HasTrigger("StatementRequirementScope_ChangeTracking"); });

            builder.HasKey(x => new { x.StatementId, x.RequirementId, x.ScopeId });

            builder
                .HasOne<Statement>(x => x.Statement)
                .WithMany(x => x.StatementsRequirementsScopes)
                .HasForeignKey(x => x.StatementId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne<Requirement>(x => x.Requirement)
                .WithMany(x => x.StatementsRequirementsScopes)
                .HasForeignKey(x => x.RequirementId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne<Scope>(x => x.Scope)
                .WithMany(x => x.StatementsRequirementsScopes)
                .HasForeignKey(x => x.ScopeId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
