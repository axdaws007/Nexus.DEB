using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class SectionRequirementConfiguration : IEntityTypeConfiguration<SectionRequirement>
    {
        public void Configure(EntityTypeBuilder<SectionRequirement> builder)
        {
            builder.ToTable("SectionRequirement", "deb");

            builder.HasKey(x => new { x.SectionID, x.RequirementID });

            builder
                .HasOne<Section>(x => x.Section)
                .WithMany(x => x.SectionRequirements)
                .HasForeignKey(x => x.SectionID)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne<Requirement>(x => x.Requirement)
                .WithMany(x => x.SectionRequirements)
                .HasForeignKey(x => x.RequirementID)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
