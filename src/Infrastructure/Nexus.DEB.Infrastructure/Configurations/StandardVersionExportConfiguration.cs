using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class StandardVersionExportConfiguration : IEntityTypeConfiguration<StandardVersionExport>
    {
        public void Configure(EntityTypeBuilder<StandardVersionExport> builder)
        {
            // Map to the database view
            builder.ToView("vw_StandardVersionExport", "deb");

            // This is a read-only view, so it has no key
            // We'll use a composite "key" for EF Core tracking purposes
            builder.HasNoKey();

            // Column mappings (these should match the view column names)
            builder.Property(e => e.EntityId)
                .HasColumnName("EntityId")
                .IsRequired();

            builder.Property(e => e.StandardId)
                .HasColumnName("StandardId")
                .IsRequired();

            builder.Property(e => e.StandardTitle)
                .HasColumnName("StandardTitle")
                .IsRequired();

            builder.Property(e => e.SerialNumber)
                .HasColumnName("SerialNumber");

            builder.Property(e => e.Title)
                .HasColumnName("StandardVersionTitle")
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("Description");

            builder.Property(e => e.EffectiveStartDate)
                .HasColumnName("EffectiveStartDate")
                .IsRequired();

            builder.Property(e => e.EffectiveEndDate)
                .HasColumnName("EffectiveEndDate");

            builder.Property(e => e.LastModifiedDate)
                .HasColumnName("LastModifiedDate")
                .IsRequired();

            builder.Property(e => e.Reference)
                .HasColumnName("Reference")
                .IsRequired();

            builder.Property(e => e.StatusId)
                .HasColumnName("StatusId")
                .IsRequired();

            builder.Property(e => e.Status)
                .HasColumnName("Status");

            builder.Property(e => e.NumberOfLinkedScopes)
                .HasColumnName("ScopeCount")
                .IsRequired();
        }
    }
}
