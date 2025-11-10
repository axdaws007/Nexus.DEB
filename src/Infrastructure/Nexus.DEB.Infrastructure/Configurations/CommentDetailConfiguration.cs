using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class CommentDetailConfiguration : IEntityTypeConfiguration<CommentDetail>
    {
        public void Configure(EntityTypeBuilder<CommentDetail> builder)
        {
            // Map to the database view
            builder.ToView("vw_CommentDetail", "common");

            // This is a read-only view, so it has no key
            // We'll use a composite "key" for EF Core tracking purposes
            builder.HasNoKey();

            // Column mappings (these should match the view column names)
            builder.Property(e => e.Id)
                .HasColumnName("Id")
                .IsRequired();

            builder.Property(e => e.EntityId)
                .HasColumnName("EntityId")
                .IsRequired();

            builder.Property(e => e.Text)
                .HasColumnName("Text")
                .IsRequired();

            builder.Property(e => e.CreatedDate)
                .HasColumnName("CreatedDate")
                .IsRequired();

            builder.Property(e => e.CreatedByUserName)
                .HasColumnName("CreatedByUserName");

            builder.Property(e => e.CreatedByPost)
                .HasColumnName("CreatedByPostTitle");

            builder.Property(e => e.CreatedByFirstName)
                .HasColumnName("CreatedByFirstName");

            builder.Property(e => e.CreatedByLastName)
                .HasColumnName("CreatedByLastName");
        }
    }
}
