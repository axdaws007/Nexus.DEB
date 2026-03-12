using Nexus.DEB.Application.Common.Models.Compliance;
using Nexus.DEB.Domain.Models;
using Task = System.Threading.Tasks.Task;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IComplianceTreeService
    {
        // ── Single-node operations ──

        Task<ComplianceTreeNode?> GetNodeAsync(
            TreeIdentifier tree, string nodeType, Guid entityId, Guid? parentEntityId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all tree node rows for a given entity within a specific tree
        /// (one row per parent relationship).
        /// </summary>
        Task<IReadOnlyList<ComplianceTreeNode>> GetNodesByEntityAsync(
            TreeIdentifier tree, string nodeType, Guid entityId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all tree node rows for a given entity across ALL trees
        /// (all Standard Version + Scope combinations).
        /// Used when an entity's workflow transitions.
        /// </summary>
        Task<IReadOnlyList<ComplianceTreeNode>> GetNodesByEntityAcrossTreesAsync(
            Guid entityId, string nodeType,
            CancellationToken cancellationToken = default);

        // ── Children queries ──

        Task<IReadOnlyList<ComplianceTreeNode>> GetChildrenAsync(
            TreeIdentifier tree, Guid parentEntityId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all descendant Requirement nodes recursively through Sections.
        /// Used for aggregate calculations within a specific tree.
        /// </summary>
        Task<IReadOnlyList<ComplianceTreeNode>> GetDescendantRequirementsAsync(
            TreeIdentifier tree, Guid ancestorEntityId,
            CancellationToken cancellationToken = default);

        // ── Mutations ──

        Task UpsertNodeAsync(ComplianceTreeNode node, CancellationToken cancellationToken = default);
        Task UpsertNodesAsync(IEnumerable<ComplianceTreeNode> nodes, CancellationToken cancellationToken = default);
        Task RemoveNodeAsync(TreeIdentifier tree, string nodeType, Guid entityId, Guid? parentEntityId, CancellationToken cancellationToken = default);
        Task RemoveNodesByEntityAsync(TreeIdentifier tree, string nodeType, Guid entityId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes all nodes for an entire tree (Standard Version + Scope).
        /// Used before a full rebuild.
        /// </summary>
        Task RemoveTreeAsync(TreeIdentifier tree, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes all trees for a given Scope across all Standard Versions.
        /// Used when a Scope is deleted.
        /// </summary>
        Task RemoveTreesByScopeAsync(Guid scopeId, CancellationToken cancellationToken = default);

        // ── Summary operations ──

        Task ReplaceSummariesAsync(
            long complianceTreeNodeId,
            IEnumerable<ComplianceTreeNodeSummary> summaries,
            CancellationToken cancellationToken = default);

        // ── Full tree query ──

        /// <summary>
        /// Gets the entire compliance tree with summaries for a Standard Version + Scope.
        /// </summary>
        Task<IReadOnlyList<ComplianceTreeNode>> GetTreeAsync(
            TreeIdentifier tree,
            CancellationToken cancellationToken = default);

        // ── Tree existence ──

        /// <summary>
        /// Gets all distinct tree identifiers (SV + Scope combos) that contain a given entity.
        /// Used to determine which trees need recalculating when an entity changes.
        /// </summary>
        Task<IReadOnlyList<TreeIdentifier>> GetTreesContainingEntityAsync(
            Guid entityId, string nodeType,
            CancellationToken cancellationToken = default);
    }
}
