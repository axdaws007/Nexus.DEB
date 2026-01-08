using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class MyWorkSummaryItemConfiguration : IEntityTypeConfiguration<MyWorkSummaryItem>
    {
        public void Configure(EntityTypeBuilder<MyWorkSummaryItem> builder)
        {
            builder.HasNoKey();
            builder.ToTable((string?)null);

            builder.Property(e => e.PostId)
                .HasColumnName("PostID");

            builder.Property(e => e.PostTitle)
                .HasColumnName("PostTitle");

            builder.Property(e => e.EntityTypeTitle)
                .HasColumnName("EntityTypeTitle");

            builder.Property(e => e.FormCount)
                .HasColumnName("FormCount");
        }
    }
}
