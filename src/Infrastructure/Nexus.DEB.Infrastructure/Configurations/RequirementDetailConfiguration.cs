using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class RequirementDetailConfiguration : IEntityTypeConfiguration<RequirementDetailView>
    {
        public void Configure(EntityTypeBuilder<RequirementDetailView> builder)
        {
            // Map to the database view
            builder.ToView("vw_RequirementDetail", "deb");

            // This is a read-only view, so it has no key
            // We'll use a composite "key" for EF Core tracking purposes
            builder.HasNoKey();

            // Column mappings (these should match the view column names)
            builder.Property(e => e.EntityId)
                .HasColumnName("EntityId")
                .IsRequired();

            builder.Property(e => e.Title)
                .HasColumnName("Title")
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("Description");

            builder.Property(e => e.SerialNumber)
                .HasColumnName("SerialNumber");

            builder.Property(e => e.CreatedDate)
                .HasColumnName("CreatedDate")
                .IsRequired();

            builder.Property(e => e.LastModifiedDate)
                .HasColumnName("LastModifiedDate")
                .IsRequired();

            builder.Property(e => e.IsRemoved)
                .HasColumnName("IsRemoved")
                .IsRequired();

            builder.Property(e => e.IsArchived)
                .HasColumnName("IsArchived")
                .IsRequired();

            builder.Property(e => e.EntityTypeTitle)
                .HasColumnName("EntityTypeTitle")
                .IsRequired();

            builder.Property(e => e.ComplianceWeighting)
                .HasColumnName("ComplianceWeighting");

            builder.Property(e => e.EffectiveStartDate)
                .HasColumnName("EffectiveStartDate");

            builder.Property(e => e.EffectiveEndDate)
                .HasColumnName("EffectiveEndDate");

            builder.Property(e => e.IsReferenceDisplayed)
                .HasColumnName("IsReferenceDisplayed");

            builder.Property(e => e.IsTitleDisplayed)
                .HasColumnName("IsTitleDisplayed");

            builder.Property(e => e.RequirementCategoryId)
                .HasColumnName("RequirementCategoryId");

            builder.Property(e => e.RequirementTypeId)
                .HasColumnName("RequirementTypeId");

            builder.Property(e => e.CreatedBy)
                .HasColumnName("CreatedBy");

            builder.Property(e => e.LastModifiedBy)
                .HasColumnName("LastModifiedBy");

            builder.Property(e => e.OwnedBy)
                .HasColumnName("OwnedBy");

            builder.Property(e => e.OwnedById)
                .HasColumnName("OwnedById");
        }
    }
}
