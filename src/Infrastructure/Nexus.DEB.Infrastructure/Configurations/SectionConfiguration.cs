using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class SectionConfiguration : IEntityTypeConfiguration<Section>
    {
        public void Configure(EntityTypeBuilder<Section> builder)
        {
            builder.ToTable("Section", "deb");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Reference).HasMaxLength(50);
            builder.Property(x => x.Title).HasMaxLength(500);
        }
    }
}
