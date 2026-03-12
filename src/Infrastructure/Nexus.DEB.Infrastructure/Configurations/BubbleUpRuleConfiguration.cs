using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class BubbleUpRuleConfiguration : IEntityTypeConfiguration<BubbleUpRule>
    {
        public void Configure(EntityTypeBuilder<BubbleUpRule> builder)
        {
            builder.ToTable("BubbleUpRule", "compliance");

            builder.HasKey(x => x.BubbleUpRuleID);

            builder.Property(x => x.ParentNodeType).HasMaxLength(50);
            builder.Property(x => x.Quantifier).HasMaxLength(10);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.ChildComplianceState)
                .WithMany()
                .HasForeignKey(x => x.ChildComplianceStateID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ResultComplianceState)
                .WithMany()
                .HasForeignKey(x => x.ResultComplianceStateID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(e => new { e.ParentNodeType, e.Ordinal })
                .IsUnique()
                .HasDatabaseName("UQ_BubbleUpRule_Ordinal");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_BubbleUpRule_Quantifier",
                "[Quantifier] IN ('Any', 'All')"
                ));
        }
    }
}
