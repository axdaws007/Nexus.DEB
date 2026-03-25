using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models.Other;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class ComplianceTreeBuildConfiguration : IEntityTypeConfiguration<ComplianceTreeBuild>
    {
        public void Configure(EntityTypeBuilder<ComplianceTreeBuild> builder)
        {
            builder.ToTable("ComplianceTreeBuild", "compliance");

            builder.HasKey(x => new { x.StandardVersionID, x.ScopeID });

            builder.Property(x => x.LiveBuildId).IsRequired();

            builder.Property(x => x.PromotedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        }
    }
}
