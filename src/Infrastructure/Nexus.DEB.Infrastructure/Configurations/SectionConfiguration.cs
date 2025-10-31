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

            builder
                .HasMany<Section>(x => x.ChildSections)
                .WithOne(x => x.ParentSection)
                .HasForeignKey(x => x.ParentSectionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne<StandardVersion>(x => x.StandardVersion)
                .WithMany()
                .HasForeignKey(x => x.StandardVersionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasMany(x => x.SectionRequirements)
                .WithOne(x => x.Section)
                .HasForeignKey(x => x.SectionID)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
