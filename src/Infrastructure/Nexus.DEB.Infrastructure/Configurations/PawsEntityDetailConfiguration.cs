using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class PawsEntityDetailConfiguration : IEntityTypeConfiguration<PawsEntityDetail>
    {
        public void Configure(EntityTypeBuilder<PawsEntityDetail> builder)
        {
            // Map to the database view
            builder.ToView("vw_PawsEntityDetail", "common");

            // This is a read-only view, so it has no key
            // We'll use a composite "key" for EF Core tracking purposes
            builder.HasNoKey();

            // Column mappings (these should match the view column names)
            builder.Property(e => e.EntityId)
                .HasColumnName("EntityID")
                .IsRequired();

            builder.Property(e => e.StepId)
                .HasColumnName("StepID")
                .IsRequired();

            builder.Property(e => e.ActivityId)
                .HasColumnName("ActivityID")
                .IsRequired();

            builder.Property(e => e.ActivityTitle)
                .HasColumnName("ActivityTitle")
                .IsRequired();

            builder.Property(e => e.StatusId)
                .HasColumnName("StatusID")
                .IsRequired();

            builder.Property(e => e.StatusTitle)
                .HasColumnName("StatusTitle")
                .IsRequired();

            builder.Property(e => e.PseudoStateId)
                .HasColumnName("PseudoStateID");

            builder.Property(e => e.PseudoStateTitle)
                .HasColumnName("PseudoStateTitle");
        }
    }
}
