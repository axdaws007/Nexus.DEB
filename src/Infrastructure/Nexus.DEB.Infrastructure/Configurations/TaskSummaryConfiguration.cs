using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class TaskSummaryConfiguration : IEntityTypeConfiguration<TaskSummary>
    {
        public void Configure(EntityTypeBuilder<TaskSummary> builder)
        {
            // Map to the database view
            builder.ToView("vw_TaskSummary", "deb");

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

            builder.Property(e => e.OwnedById)
                .HasColumnName("OwnedById")
                .IsRequired();

            builder.Property(e => e.OwnedBy)
                .HasColumnName("OwnedBy");

            builder.Property(e => e.DueDate)
                .HasColumnName("DueDate");

            builder.Property(e => e.OriginalDueDate)
                .HasColumnName("OriginalDueDate");

            builder.Property(e => e.TaskTypeId)
                .HasColumnName("TaskTypeId")
                .IsRequired();

            builder.Property(e => e.TaskTypeTitle)
                .HasColumnName("TaskTypeTitle")
                .IsRequired();

            builder.Property(e => e.StatusId)
                .HasColumnName("StatusId");

            builder.Property(e => e.Status)
                .HasColumnName("Status");

            builder.Property(e => e.StatementId)
                .HasColumnName("StatementId")
                .IsRequired();

            builder.Property(e => e.EntityTypeTitle)
                .HasColumnName("EntityTypeTitle")
                .IsRequired();
        }
    }
}
