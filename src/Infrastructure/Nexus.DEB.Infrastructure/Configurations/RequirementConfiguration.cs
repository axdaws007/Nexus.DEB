using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class RequirementConfiguration : IEntityTypeConfiguration<Requirement>
    {
        public void Configure(EntityTypeBuilder<Requirement> builder)
        {
            builder.ToTable("Requirement", "deb");

            builder.HasBaseType<EntityHead>();

            builder
                .HasOne<RequirementCategory>(x => x.RequirementCategory)
                .WithMany()
                .HasForeignKey(x => x.RequirementCategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne<RequirementType>(x => x.RequirementType)
                .WithMany()
                .HasForeignKey(x => x.RequirementTypeId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
