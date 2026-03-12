using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class PseudostateMappingConfiguration : IEntityTypeConfiguration<PseudostateMapping>
    {
        public void Configure(EntityTypeBuilder<PseudostateMapping> builder)
        {
            builder.ToTable("PseudostateMapping", "compliance");

            builder.HasKey(x => x.PseudostateMappingID);

            builder.Property(x => x.EntityType).HasMaxLength(50);
            builder.Property(x => x.PseudoStateTitle).HasMaxLength(100);

            builder.HasOne(x => x.ComplianceState)
                .WithMany()
                .HasForeignKey(x => x.ComplianceStateID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(e => new { e.EntityType, e.PseudoStateID })
                .IsUnique()
                .HasDatabaseName("UQ_PseudostateMapping");
        }
    }
}
