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
	public class StandardVersionRequirementConfiguration : IEntityTypeConfiguration<StandardVersionRequirement>
	{
		public void Configure(EntityTypeBuilder<StandardVersionRequirement> builder)
		{
			// Map to the database view
			builder.ToView("vw_StandardVersionRequirements", "deb");

			// This is a read-only view, so it has no key
			// We'll use a composite "key" for EF Core tracking purposes
			builder.HasNoKey();

			builder.Property(e => e.RequirementId)
				.HasColumnName("RequirementId")
				.IsRequired();

			builder.Property(e => e.SerialNumber)
				.HasColumnName("SerialNumber")
				.IsRequired();

			builder.Property(e => e.Title)
				.HasColumnName("Title")
				.IsRequired();

			builder.Property(e => e.StandardVersionId)
				.HasColumnName("StandardVersionId");

			builder.Property(e => e.StandardVersion)
				.HasColumnName("StandardVersion");

			builder.Property(e => e.SectionId)
				.HasColumnName("SectionId");

			builder.Property(e => e.Section)
				.HasColumnName("Section");
		}
	}
}
