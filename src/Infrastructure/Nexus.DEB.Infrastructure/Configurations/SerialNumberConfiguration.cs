using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class SerialNumberConfiguration : IEntityTypeConfiguration<SerialNumber>
    {
        public void Configure(EntityTypeBuilder<SerialNumber> builder)
        {
            builder.ToTable("SerialNumber");

            builder.HasKey(x => x.SerialNumberId);

            builder.HasIndex(x => new { x.ModuleId, x.InstanceId, x.EntityType });

            builder.Property(x => x.EntityType).HasMaxLength(50);
            builder.Property(x => x.Format).HasMaxLength(150);
        }
    }
}
