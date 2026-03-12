using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models.Other;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class ComplianceStateMappingConfiguration : IEntityTypeConfiguration<ComplianceStateMapping>
    {
        public void Configure(EntityTypeBuilder<ComplianceStateMapping> builder)
        {
            builder.ToTable("ComplianceStateMapping", "compliance");

            builder.HasKey(x => x.ComplianceStateMappingID);

            builder.Property(x => x.ActivityTitle).HasMaxLength(100);
            builder.Property(x => x.StatusTitle).HasMaxLength(100);

            builder.HasOne(x => x.ComplianceState)
                .WithMany()
                .HasForeignKey(x => x.ComplianceStateID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(e => new { e.WorkflowID, e.ActivityID, e.StatusID })
                .IsUnique()
                .HasDatabaseName("UQ_ComplianceStateMapping");
        }
    }
}
