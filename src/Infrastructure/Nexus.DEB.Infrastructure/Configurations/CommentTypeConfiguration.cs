using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class CommentTypeConfiguration : IEntityTypeConfiguration<CommentType>
    {
        public void Configure(EntityTypeBuilder<CommentType> builder)
        {
            builder.ToTable("CommentType", "common");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Title).HasMaxLength(50);
            builder.Property(x => x.Description).HasMaxLength(500);
        }
    }
}
