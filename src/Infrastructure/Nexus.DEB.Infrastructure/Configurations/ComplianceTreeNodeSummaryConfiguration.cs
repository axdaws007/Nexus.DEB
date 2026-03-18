using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class ComplianceTreeNodeSummaryConfiguration : IEntityTypeConfiguration<ComplianceTreeNodeSummary>
    {
        public void Configure(EntityTypeBuilder<ComplianceTreeNodeSummary> builder)
        {
            builder.ToTable("ComplianceTreeNodeSummary", "compliance");

            builder.HasKey(x => new { x.ComplianceTreeNodeID, x.ChildNodeType, x.ComplianceStateID });

            builder.Property(x => x.ChildNodeType).HasMaxLength(50);

            builder.Property(x => x.Count).HasDefaultValue(0);

            builder.HasOne(x => x.TreeNode)
                  .WithMany(x => x.Summaries)
                  .HasForeignKey(e => e.ComplianceTreeNodeID)
                  .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ComplianceState)
                .WithMany()
                .HasForeignKey(x => x.ComplianceStateID)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
