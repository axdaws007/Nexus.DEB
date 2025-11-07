using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models.Other;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class ModuleSettingConfiguration : IEntityTypeConfiguration<ModuleSetting>
    {
        public void Configure(EntityTypeBuilder<ModuleSetting> builder)
        {
            builder.ToTable("ModuleSetting");

            builder.HasKey(x => new { x.ModuleId, x.Name });
            builder.Property(x => x.Name).HasMaxLength(100);

            builder.HasOne(x => x.Type)
                .WithMany()
                .HasForeignKey(x => x.TypeId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
