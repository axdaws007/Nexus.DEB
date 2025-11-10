using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments", "common");

            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.EntityId);

            builder.Property(x => x.CreatedByUserName).HasMaxLength(255);
            builder.Property(x => x.CreatedByPostTitle).HasMaxLength(255);

            builder.HasOne(x => x.CommentType)
                .WithMany()
                .HasForeignKey(x => x.CommentTypeId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
