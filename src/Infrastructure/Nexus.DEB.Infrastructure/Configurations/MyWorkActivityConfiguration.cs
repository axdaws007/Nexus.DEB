using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    internal class MyWorkActivityConfiguration : IEntityTypeConfiguration<MyWorkActivity>
    {
        public void Configure(EntityTypeBuilder<MyWorkActivity> builder)
        {
            builder.HasNoKey();
            builder.ToTable((string?)null);

            builder.Property(e => e.ActivityID)
                .HasColumnName("ActivityID");

            builder.Property(e => e.ActivityTitle)
                .HasColumnName("ActivityTitle");

            builder.Property(e => e.FormCount)
                .HasColumnName("FormCount");
        }
    }
}
