using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class ViewPostConfiguration : IEntityTypeConfiguration<ViewPost>
    {
        public void Configure(EntityTypeBuilder<ViewPost> builder)
        {
            // Map to the database view
            builder.ToView("XDB_CIS_View_Post", "common");

            // This is a read-only view, so it has no key
            // We'll use a composite "key" for EF Core tracking purposes
            builder.HasNoKey();

            // Column mappings (these should match the view column names)
            builder.Property(e => e.Id)
                .HasColumnName("ID")
                .IsRequired();

            builder.Property(e => e.Title)
                .HasColumnName("Title")
                .IsRequired();

            builder.Property(e => e.PostTypeId)
                .HasColumnName("PostTypeID")
                .IsRequired();

            builder.Property(e => e.IsDeleted)
                .HasColumnName("IsDeleted")
                .IsRequired();
        }
    }
}
