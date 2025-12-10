using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Infrastructure.Configurations
{
	public class StandardVersionDetailViewConfiguration : IEntityTypeConfiguration<StandardVersionDetailView>
	{
		public void Configure(EntityTypeBuilder<StandardVersionDetailView> builder)
		{
			builder.ToView("vw_StandardVersionDetail", "deb");

			builder.HasNoKey();

			builder.Property(e => e.EntityId)
				.HasColumnName("EntityId")
				.IsRequired();
			builder.Property(e => e.EntityTypeTitle)
				.HasColumnName("EntityTypeTitle")
				.IsRequired();
			builder.Property(e => e.SerialNumber)
				.HasColumnName("SerialNumber");
			builder.Property(e => e.StandardId)
				.HasColumnName("StandardId")
				.IsRequired();
			builder.Property(e => e.StandardTitle)
				.HasColumnName("StandardTitle")
				.IsRequired();
			builder.Property(e => e.Delimiter)
				.HasColumnName("Delimiter")
				.IsRequired();
			builder.Property(e => e.Title)
				.HasColumnName("Title")
				.IsRequired();
			builder.Property(e => e.Description)
				.HasColumnName("Description");
			builder.Property(e => e.CreatedDate)
				.HasColumnName("CreatedDate")
				.IsRequired();
			builder.Property(e => e.CreatedBy)
				.HasColumnName("CreatedByPostTitle")
				.IsRequired();
			builder.Property(e => e.OwnedBy)
				.HasColumnName("OwnedByPostTitle")
				.IsRequired();
			builder.Property(e => e.LastModifiedBy)
				.HasColumnName("LastModifiedByPostTitle")
				.IsRequired();
			builder.Property(e => e.LastModifiedDate)
				.HasColumnName("LastModifiedDate")
				.IsRequired();
			builder.Property(e => e.MajorVersion)
				.HasColumnName("MajorVersion")
				.IsRequired();
			builder.Property(e => e.MinorVersion)
				.HasColumnName("MinorVersion")
				.IsRequired();
			builder.Property(e => e.EffectiveStartDate)
				.HasColumnName("EffectiveStartDate")
				.IsRequired();
			builder.Property(e => e.EffectiveEndDate)
				.HasColumnName("EffectiveEndDate");
		}
	}
}
