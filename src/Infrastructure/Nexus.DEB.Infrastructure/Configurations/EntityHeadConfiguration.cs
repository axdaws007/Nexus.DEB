using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class EntityHeadConfiguration : IEntityTypeConfiguration<EntityHead>
    {
        public void Configure(EntityTypeBuilder<EntityHead> builder)
        {
            builder.ToTable("EntityHead", "common");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SerialNumber).HasMaxLength(150);
        }
    }
}
