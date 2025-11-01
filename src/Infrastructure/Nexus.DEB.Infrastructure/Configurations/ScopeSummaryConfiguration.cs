﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class ScopeSummaryConfiguration : IEntityTypeConfiguration<ScopeSummary>
    {
        public void Configure(EntityTypeBuilder<ScopeSummary> builder)
        {
            // Map to the database view
            builder.ToView("vw_ScopeSummary", "deb");

            // This is a read-only view, so it has no key
            // We'll use a composite "key" for EF Core tracking purposes
            builder.HasNoKey();

            // Column mappings (these should match the view column names)
            builder.Property(e => e.Id)
                .HasColumnName("Id")
                .IsRequired();

            builder.Property(e => e.Title)
                .HasColumnName("Title")
                .IsRequired();

            builder.Property(e => e.OwnedById)
                .HasColumnName("OwnedById")
                .IsRequired();

            builder.Property(e => e.CreatedDate)
                .HasColumnName("CreatedDate")
                .IsRequired();

            builder.Property(e => e.LastModifiedDate)
                .HasColumnName("LastModifiedDate")
                .IsRequired();

            builder.Property(e => e.NumberOfLinkedStandardVersions)
                .HasColumnName("StandardVersionCount");
        }
    }
}
