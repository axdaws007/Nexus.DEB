using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class StatementDetailConfiguration : IEntityTypeConfiguration<StatementDetailView>
    {
        public void Configure(EntityTypeBuilder<StatementDetailView> builder)
        {
            // Map to the database view
            builder.ToView("vw_StatementDetail", "deb");

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

            builder.Property(e => e.ReviewDate)
                .HasColumnName("ReviewDate");

            builder.Property(e => e.ScopeID)
                .HasColumnName("ScopeID")
                .IsRequired();

            builder.Property(e => e.Scope)
                .HasColumnName("Scope");

            builder.Property(e => e.StatementText)
                .HasColumnName("StatementText")
                .IsRequired();

            builder.Property(e => e.CreatedBy)
                .HasColumnName("CreatedBy");

            builder.Property(e => e.LastModifiedBy)
                .HasColumnName("LastModifiedBy");

            builder.Property(e => e.OwnedBy)
                .HasColumnName("OwnedBy");
        }
    }
}
