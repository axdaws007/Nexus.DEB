using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models.Other;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class SettingsTypeConfiguration : IEntityTypeConfiguration<SettingsType>
    {
        public void Configure(EntityTypeBuilder<SettingsType> builder)
        {
            builder.ToTable("SettingsType");

            builder.HasKey(x => x.TypeId);
            builder.Property(x => x.TypeName).HasMaxLength(100);
        }
    }
}
