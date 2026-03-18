using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class NodeDefaultConfiguration : IEntityTypeConfiguration<NodeDefault>
    {
        public void Configure(EntityTypeBuilder<NodeDefault> builder)
        {
            builder.ToTable("NodeDefault", "compliance");

            builder.HasKey(x => x.NodeDefaultID);

            builder.Property(x => x.NodeType).HasMaxLength(50);
            builder.Property(x => x.Scenario).HasMaxLength(50).HasDefaultValue("NoChildren");
            builder.Property(x => x.DefaultLabel).HasMaxLength(200);

            builder.HasOne(x => x.DefaultComplianceState)
                .WithMany()
                .HasForeignKey(x => x.DefaultComplianceStateID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(e => new { e.NodeType, e.Scenario })
                .IsUnique()
                .HasDatabaseName("UQ_NodeDefault");
        }
    }
}
