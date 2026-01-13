using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class MyWorkDetailItemConfiguration : IEntityTypeConfiguration<MyWorkDetailItem>
    {
        public void Configure(EntityTypeBuilder<MyWorkDetailItem> builder)
        {
            builder.HasNoKey();
            builder.ToTable((string?)null);

            builder.Property(e => e.ModuleID)
                .HasColumnName("ModuleID");

            builder.Property(e => e.EntityTypeTitle)
                .HasColumnName("EntityTypeTitle");

            builder.Property(e => e.EntityID)
                .HasColumnName("EntityID");

            builder.Property(e => e.SerialNumber)
                .HasColumnName("SerialNumber");

            builder.Property(e => e.Title)
                .HasColumnName("Title");

            builder.Property(e => e.CreatedDate)
                .HasColumnName("CreatedDate");

            builder.Property(e => e.ModifiedDate)
                .HasColumnName("ModifiedDate");

            builder.Property(e => e.DueDate)
                .HasColumnName("DueDate")
                .IsRequired(false);

            builder.Property(e => e.ReviewDate)
                .HasColumnName("ReviewDate")
                .IsRequired(false);

            builder.Property(e => e.PendingActivityList)
                .HasColumnName("PendingActivityList");

            builder.Property(e => e.PendingActivityOwners)
                .HasColumnName("PendingActivityOwners");

            builder.Property(e => e.OwnerGroup)
                .HasColumnName("OwnerGroup")
                .IsRequired(false);

            builder.Property(e => e.OwnerPost)
                .HasColumnName("OwnerPost");

            builder.Property(e => e.TransferDates)
                .HasColumnName("TransferDates");
        }
    }
}
