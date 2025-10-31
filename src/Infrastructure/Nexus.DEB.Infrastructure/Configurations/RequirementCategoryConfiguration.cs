using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class RequirementCategoryConfiguration : IEntityTypeConfiguration<RequirementCategory>
    {
        public void Configure(EntityTypeBuilder<RequirementCategory> builder)
        {
            builder.ToTable("RequirementCategory", "deb");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Title).HasMaxLength(50);
            builder.Property(x => x.Description).HasMaxLength(500);
        }
    }
}
