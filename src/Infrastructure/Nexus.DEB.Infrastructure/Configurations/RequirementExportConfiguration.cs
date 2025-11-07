using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class RequirementExportConfiguration : IEntityTypeConfiguration<RequirementExport>
    {
        public void Configure(EntityTypeBuilder<RequirementExport> builder)
        {
            // Map to the database view
            builder.ToView("vw_RequirementExport", "deb");

            // This is a read-only view, so it has no key
            // We'll use a composite "key" for EF Core tracking purposes
            builder.HasNoKey();

            // Column mappings (these should match the view column names)
            builder.Property(e => e.EntityId)
                .HasColumnName("EntityId")
                .IsRequired();

            builder.Property(e => e.SerialNumber)
                .HasColumnName("SerialNumber");

            builder.Property(e => e.Title)
                .HasColumnName("Title")
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("Description");

            builder.Property(e => e.LastModifiedDate)
                .HasColumnName("LastModifiedDate")
                .IsRequired();

            builder.Property(e => e.SectionReferences)
                .HasColumnName("SectionReferences");

            builder.Property(e => e.StatusId)
                .HasColumnName("StatusId");

            builder.Property(e => e.Status)
                .HasColumnName("Status");

            builder.Property(e => e.EffectiveStartDate)
                .HasColumnName("EffectiveStartDate")
                .IsRequired();

            builder.Property(e => e.EffectiveEndDate)
                .HasColumnName("EffectiveEndDate")
                .IsRequired();

            builder.Property(e => e.RequirementCategoryTitle)
                .HasColumnName("RequirementCategoryTitle")
                .IsRequired();

            builder.Property(e => e.RequirementTypeTitle)
                .HasColumnName("RequirementTypeTitle")
                .IsRequired();

            builder.Property(e => e.ComplianceWeighting)
                .HasColumnName("ComplianceWeighting");
        }
    }
}