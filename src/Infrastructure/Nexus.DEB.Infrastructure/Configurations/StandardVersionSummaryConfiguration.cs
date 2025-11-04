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
    public class StandardVersionSummaryConfiguration : IEntityTypeConfiguration<StandardVersionSummary>
    {
        public void Configure(EntityTypeBuilder<StandardVersionSummary> builder)
        {
            // Map to the database view
            builder.ToView("vw_StandardVersionSummary", "deb");

            // This is a read-only view, so it has no key
            // We'll use a composite "key" for EF Core tracking purposes
            builder.HasNoKey();

            // Column mappings (these should match the view column names)
            builder.Property(e => e.EntityId)
                .HasColumnName("EntityId")
                .IsRequired();

            builder.Property(e => e.StandardId)
                .HasColumnName("StandardId")
                .IsRequired();

            builder.Property(e => e.StandardTitle)
                .HasColumnName("StandardTitle")
                .IsRequired();

            builder.Property(e => e.Version)
                .HasColumnName("Version")
                .IsRequired();

            builder.Property(e => e.Title)
                .HasColumnName("StandardVersionTitle")
                .IsRequired();

            builder.Property(e => e.EffectiveFrom)
                .HasColumnName("EffectiveStartDate")
                .IsRequired();

            builder.Property(e => e.EffectiveTo)
                .HasColumnName("EffectiveEndDate");

            builder.Property(e => e.LastModifiedDate)
                .HasColumnName("LastModifiedDate")
                .IsRequired();

            builder.Property(e => e.StatusId)
                .HasColumnName("StatusId")
                .IsRequired();

            builder.Property(e => e.Status)
                .HasColumnName("Status");

            builder.Property(e => e.NumberOfLinkedScopes)
                .HasColumnName("ScopeCount")
                .IsRequired();
        }
    }
}
