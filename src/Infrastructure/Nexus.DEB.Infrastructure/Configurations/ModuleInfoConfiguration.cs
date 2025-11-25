using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models.Other;

namespace Nexus.DEB.Infrastructure.Configurations
{
	public class ModuleInfoConfiguration : IEntityTypeConfiguration<ModuleInfo>
	{
		public void Configure(EntityTypeBuilder<ModuleInfo> builder)
		{
			builder.ToTable("ModuleInfo");

			builder.HasKey(x => x.ModuleId);
			builder.Property(x => x.ModuleName).HasMaxLength(200);
			builder.Property(x => x.AssemblyName).HasMaxLength(200);
			builder.Property(x => x.IOCName).HasMaxLength(200);
			builder.Property(x => x.SchemaName).HasMaxLength(50);

			builder.HasMany<ModuleSetting>(builder => builder.ModuleSettings)
				.WithOne(module => module.ModuleInfo)
				.HasForeignKey(builder => builder.ModuleId);
		}
	}
}
