using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class EntityHeadDetailConfiguration : IEntityTypeConfiguration<EntityHeadDetail>
    {
        public void Configure(EntityTypeBuilder<EntityHeadDetail> builder)
        {
            // Map to the database view
            builder.ToView("vw_EntityHeadDetail", "common");

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

            builder.Property(e => e.CreatedDate)
                .HasColumnName("CreatedDate");

            builder.Property(e => e.CreatedByPostTitle)
                .HasColumnName("CreatedByPostTitle");

            builder.Property(e => e.LastModifiedDate)
                .HasColumnName("LastModifiedDate")
                .IsRequired();

            builder.Property(e => e.LastModifiedByPostTitle)
                .HasColumnName("LastModifiedByPostTitle");

            builder.Property(e => e.OwnedByPostTitle)
                .HasColumnName("OwnedByPostTitle");

            builder.Property(e => e.SerialNumber)
                .HasColumnName("SerialNumber");

            builder.Property(e => e.EntityTypeTitle)
                .HasColumnName("EntityTypeTitle")
                .IsRequired();
        }
    }
}
