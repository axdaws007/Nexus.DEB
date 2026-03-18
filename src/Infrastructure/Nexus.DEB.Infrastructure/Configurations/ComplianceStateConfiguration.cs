using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class ComplianceStateConfiguration : IEntityTypeConfiguration<ComplianceState>
    {
        public void Configure(EntityTypeBuilder<ComplianceState> builder)
        {
            builder.ToTable("ComplianceState", "compliance");

            builder.HasKey(x => x.ComplianceStateID);

            builder.Property(x => x.Name).HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.Colour).HasMaxLength(7);

            builder.Property(x => x.IsTerminal).HasDefaultValue(false);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("UQ_ComplianceState_Name");
        }
    }
}
