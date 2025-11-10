using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class PawsStateConfiguration : IEntityTypeConfiguration<PawsState>
    {
        public void Configure(EntityTypeBuilder<PawsState> builder)
        {
            // Map to the database view
            builder.ToView("vwPawsState", "common");

            // This is a read-only view, so it has no key
            // We'll use a composite "key" for EF Core tracking purposes
            builder.HasNoKey();

            // Column mappings (these should match the view column names)
            builder.Property(e => e.EntityId)
                .HasColumnName("EntityID")
                .IsRequired();

            builder.Property(e => e.StatusId)
                .HasColumnName("StateID");

            builder.Property(e => e.Status)
                .HasColumnName("StateTitle");

            builder.Property(e => e.WorkflowId)
                .HasColumnName("ProcessTemplateID")
                .IsRequired();
        }
    }
}
