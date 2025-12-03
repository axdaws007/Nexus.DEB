using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class EntityHeadConfiguration : IEntityTypeConfiguration<EntityHead>
    {
        public void Configure(EntityTypeBuilder<EntityHead> builder)
        {
            builder.ToTable("EntityHead", "common", t => { t.HasTrigger("EntityHead_ChangeTracking"); });

            builder.HasKey(x => x.EntityId);
            builder.HasIndex(x => x.EntityTypeTitle);
            builder.HasIndex(x => x.CreatedDate);
            builder.HasIndex(x => x.LastModifiedDate);
            builder.HasIndex(x => x.OwnedById);

            builder.Property(x => x.EntityTypeTitle).HasMaxLength(50);
            builder.Property(x => x.SerialNumber).HasMaxLength(150);
        }
    }
}
