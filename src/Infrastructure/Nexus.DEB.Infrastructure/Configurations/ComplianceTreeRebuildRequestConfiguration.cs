using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models.Other;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class ComplianceTreeRebuildRequestConfiguration : IEntityTypeConfiguration<ComplianceTreeRebuildRequest>
    {
        public void Configure(EntityTypeBuilder<ComplianceTreeRebuildRequest> builder)
        {
            builder.ToTable("ComplianceTreeRebuildRequest", "compliance");

            builder.HasKey(x => new { x.StandardVersionID, x.ScopeID });

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.RequestedAt)
                .IsRequired();

            builder.Property(x => x.BuildId);

            builder.Property(x => x.StartedAt);

            // Index to support the Quartz job polling query:
            // "find all Pending requests where RequestedAt < debounce threshold"
            builder.HasIndex(x => new { x.Status, x.RequestedAt })
                .HasDatabaseName("IX_ComplianceTreeRebuildRequest_StatusRequestedAt");
        }
    }
}
