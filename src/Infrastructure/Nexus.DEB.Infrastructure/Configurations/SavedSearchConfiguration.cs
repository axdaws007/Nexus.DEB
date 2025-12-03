using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Infrastructure.Configurations
{
	public class SavedSearchConfiguration : IEntityTypeConfiguration<SavedSearch>
	{
		public void Configure(EntityTypeBuilder<SavedSearch> builder)
		{
			builder.ToTable("SavedSearch", "common");

			builder.HasKey(x => new { x.PostId, x.Name, x.Context, x.ModuleId });

			builder.Property(x => x.Name).HasMaxLength(350);
			builder.Property(x => x.Context).HasMaxLength(50);
			builder.Property(x => x.CreatedDate).IsRequired();
		}
	}
}
