using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Configurations
{
    public class ComplianceTreeNodeConfiguration : IEntityTypeConfiguration<ComplianceTreeNode>
    {
        public void Configure(EntityTypeBuilder<ComplianceTreeNode> builder)
        {
            builder.ToTable("ComplianceTreeNode", "compliance");

            builder.HasKey(x => x.ComplianceTreeNodeID);

            builder.Property(x => x.NodeType).HasMaxLength(50);
            builder.Property(x => x.ParentNodeType).HasMaxLength(50);
            builder.Property(x => x.ComplianceStateLabel).HasMaxLength(200);
            builder.Property(x => x.NodeLabel).HasMaxLength(150);
            builder.Property(x => x.NodeReference).HasMaxLength(150);

            builder.Property(x => x.LastCalculatedAt).HasDefaultValueSql("getdate()");
            builder.Property(x => x.Ordinal).HasDefaultValue(0);

            builder.Property(x => x.BuildId).IsRequired();

            builder.HasIndex(x => x.BuildId).HasDatabaseName("IX_ComplianceTreeNode_BuildId");

            builder.HasOne(x => x.ComplianceState)
                .WithMany()
                .HasForeignKey(x => x.ComplianceStateID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ParentComplianceTreeNode)
                .WithMany()
                .HasForeignKey(x => x.ParentComplianceTreeNodeID)
                .OnDelete(DeleteBehavior.NoAction);

            // Filtered unique: one row per entity per parent per tree, where a parent exists
            builder.HasIndex(e => new { e.StandardVersionID, e.ScopeID, e.BuildId, e.NodeType, e.EntityID, e.ParentComplianceTreeNodeID })
                .IsUnique()
                .HasFilter("[ParentComplianceTreeNodeID] IS NOT NULL")
                .HasDatabaseName("UQ_ComplianceTreeNode_WithParent");

            // Filtered unique: one root node per tree
            builder.HasIndex(e => new { e.StandardVersionID, e.ScopeID, e.BuildId, e.NodeType, e.EntityID })
                .IsUnique()
                .HasFilter("[ParentEntityID] IS NULL")
                .HasDatabaseName("UQ_ComplianceTreeNode_Root");

            builder.HasIndex(e => new { e.StandardVersionID, e.ScopeID, e.ParentNodeType, e.ParentEntityID })
                .HasDatabaseName("IX_ComplianceTreeNode_Parent");

            // Get all children of a parent within a tree
            builder.HasIndex(e => new { e.StandardVersionID, e.ScopeID, e.ParentEntityID, e.ParentNodeType })
                .IncludeProperties(e => new { e.ComplianceStateID, e.ComplianceStateLabel })
                .HasDatabaseName("IX_ComplianceTreeNode_Children");

            // Find all tree rows for a given entity across all trees (on workflow transition)
            builder.HasIndex(e => new { e.EntityID, e.NodeType })
                .IncludeProperties(e => new { e.StandardVersionID, e.ScopeID, e.ParentEntityID, e.ComplianceStateID })
                .HasDatabaseName("IX_ComplianceTreeNode_Entity");

            // Get all nodes for a specific tree (full tree query and rebuild)
            builder.HasIndex(e => e.BuildId)
                .IncludeProperties(e => new { e.NodeType, e.EntityID, e.ParentNodeType, e.ParentEntityID, e.ComplianceStateID, e.ComplianceStateLabel })
                .HasDatabaseName("IX_ComplianceTreeNode_Tree");

            // Get all parent IDs
            builder.HasIndex(e => e.ParentComplianceTreeNodeID )
                .IncludeProperties(e => new { e.ComplianceStateID, e.NodeType })
                .HasDatabaseName("IX_ComplianceTreeNode_ParentNodeID");
        }
    }
}
