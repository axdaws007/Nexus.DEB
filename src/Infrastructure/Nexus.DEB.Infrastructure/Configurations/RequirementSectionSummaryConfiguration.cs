using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models.Views;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class RequirementSectionSummaryConfiguration : IEntityTypeConfiguration<RequirementSectionSummary>
    {
        public void Configure(EntityTypeBuilder<RequirementSectionSummary> builder)
        {
            // Map to the database view
            builder.ToView("vw_RequirementSectionSummary", "deb");

            // This is a read-only view, so it has no key
            // We'll use a composite "key" for EF Core tracking purposes
            builder.HasNoKey();

            // Column mappings (these should match the view column names)
            builder.Property(e => e.RequirementId)
                .HasColumnName("EntityId")
                .IsRequired();

            builder.Property(e => e.SerialNumber)
                .HasColumnName("SerialNumber");

            builder.Property(e => e.Title)
                .HasColumnName("Title");

            builder.Property(e => e.Description)
                .HasColumnName("Description");

            builder.Property(e => e.StatusId)
                .HasColumnName("StatusId");

            builder.Property(e => e.Status)
                .HasColumnName("Status");

            builder.Property(e => e.NumberOfLinkedSections)
                .HasColumnName("NumberOfLinkedSections")
                .IsRequired();
        }
    }
}
