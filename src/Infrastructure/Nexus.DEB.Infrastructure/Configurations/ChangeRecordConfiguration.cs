using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
	public class ChangeRecordConfiguration : IEntityTypeConfiguration<ChangeRecord>
	{
		public void Configure(EntityTypeBuilder<ChangeRecord> builder)
		{
			builder.ToTable("ChangeRecord", "common", t => { t.HasTrigger("ChangeTracking");  }); // Although this table doesn't have it's own trigger we must declare an arbitrary trigger because other triggers write to the table.

			builder.HasKey(x => x.Id);
			builder.HasIndex(x => x.EntityId);

			builder.Property(x => x.ChangeDate).IsRequired();
			builder.Property(x => x.ChangeByUser).HasMaxLength(128);
			builder.Property(x => x.EventId).IsRequired();

			builder.HasMany(builder => builder.ChangeRecordItems)
				.WithOne(item => item.ChangeRecord)
				.HasForeignKey(item => item.ChangeRecordId);
		}
	}
}
