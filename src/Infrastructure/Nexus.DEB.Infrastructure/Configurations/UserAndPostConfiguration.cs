using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class UserAndPostConfiguration : IEntityTypeConfiguration<UserAndPost>
    {
        public void Configure(EntityTypeBuilder<UserAndPost> builder)
        {
            // Map to the database view
            builder.ToView("XDB_CIS_User_Post", "common");

            // This is a read-only view, so it has no key
            // We'll use a composite "key" for EF Core tracking purposes
            builder.HasNoKey();

            // Column mappings (these should match the view column names)
            builder.Property(e => e.UserId)
                .HasColumnName("userID")
                .IsRequired();

            builder.Property(e => e.UserName)
                .HasColumnName("UserName")
                .IsRequired();

            builder.Property(e => e.IsUserDeleted)
                .HasColumnName("IsUserDeleted")
                .IsRequired();

            builder.Property(e => e.IsUserEnabled)
                .HasColumnName("IsUserEnabled")
                .IsRequired();

            builder.Property(e => e.PostId)
                .HasColumnName("postID")
                .IsRequired();

            builder.Property(e => e.PostTitle)
                .HasColumnName("PostTitle")
                .IsRequired();

            builder.Property(e => e.IsPostDeleted)
                .HasColumnName("IsPostDeleted")
                .IsRequired();
        }
    }
}
