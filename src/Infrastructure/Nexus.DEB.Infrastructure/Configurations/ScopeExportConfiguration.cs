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
    public class ScopeExportConfiguration : IEntityTypeConfiguration<ScopeExport>
    {
        public void Configure(EntityTypeBuilder<ScopeExport> builder)
        {
            // Map to the database view
            builder.ToView("vw_ScopeExport", "deb");

            // This is a read-only view, so it has no key
            // We'll use a composite "key" for EF Core tracking purposes
            builder.HasNoKey();

            // Column mappings (these should match the view column names)
            builder.Property(e => e.EntityId)
                .HasColumnName("EntityId")
                .IsRequired();

            builder.Property(e => e.SerialNumber)
                .HasColumnName("SerialNumber");

            builder.Property(e => e.Title)
                .HasColumnName("Title")
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("Description");

            builder.Property(e => e.OwnedById)
                .HasColumnName("OwnedById")
                .IsRequired();

            builder.Property(e => e.OwnedBy)
                .HasColumnName("OwnedBy");

            builder.Property(e => e.CreatedDate)
                .HasColumnName("CreatedDate")
                .IsRequired();

            builder.Property(e => e.LastModifiedDate)
                .HasColumnName("LastModifiedDate")
                .IsRequired();

            builder.Property(e => e.NumberOfLinkedStandardVersions)
                .HasColumnName("StandardVersionCount");

            builder.Property(e => e.Status)
                .HasColumnName("Status");

            builder.Property(e => e.TargetImplementationDate)
                .HasColumnName("TargetImplementationDate");

        }
    }
}