using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
	public class ChangeRecordItemConfiguration : IEntityTypeConfiguration<ChangeRecordItem>
	{
		public void Configure(EntityTypeBuilder<ChangeRecordItem> builder)
		{
			builder.ToTable("ChangeRecordItem", "common", t => { t.HasTrigger("ChangeTracking"); }); // Although this table doesn't have it's own trigger we must declare an arbitrary trigger because other triggers write to the table.

			builder.HasKey(x => x.Id);

			builder.HasOne(x => x.ChangeRecord)
				.WithMany(x => x.ChangeRecordItems)
				.HasForeignKey(x => x.ChangeRecordId)
				.IsRequired()
				.OnDelete(DeleteBehavior.NoAction);
		}
	}
}
