using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using Nexus.DEB.Domain.Models.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Infrastructure.Configurations
{
	public class ScopeDetailViewConfiguration : IEntityTypeConfiguration<ScopeDetailView>
	{
		public void Configure(EntityTypeBuilder<ScopeDetailView> builder)
		{
			builder.ToView("vw_ScopeDetail", "deb");

			builder.HasNoKey();

			builder.Property(e => e.EntityId)
				.HasColumnName("EntityId")
				.IsRequired();
			builder.Property(e => e.EntityTypeTitle)
				.HasColumnName("EntityTypeTitle")
				.IsRequired();
			builder.Property(e => e.SerialNumber)
				.HasColumnName("SerialNumber");
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
			builder.Property(e => e.TargetImplementationDate)
				.HasColumnName("TargetImplementationDate");
			builder.Property(e => e.IsRemoved)
				.HasColumnName("IsRemoved")
				.IsRequired();
			builder.Property(e => e.IsArchived)
				.HasColumnName("IsArchived")
				.IsRequired();
		}
	}
}
