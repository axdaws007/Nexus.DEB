using Nexus.DEB.Application.Common.Models.Compliance;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IComplianceTreeRecalculator
    {
        /// <summary>
        /// Recalculates a single entity's compliance state (from its pseudostate)
        /// and bubbles up to the root in every tree containing this entity.
        /// Handles multi-parent and multi-tree (multi-Scope) scenarios.
        /// </summary>
        Task RecalculateFromEntityAsync(
            Guid entityId, string entityType, string nodeType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Recalculates a parent node from its children within a specific tree,
        /// then bubbles up to root. Used after structural changes.
        /// </summary>
        Task RecalculateFromParentAsync(
            TreeIdentifier tree, Guid parentEntityId, string parentNodeType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Full rebuild of a specific compliance tree (Standard Version + Scope).
        /// Clears and rebuilds from scratch. Also serves as a repair tool.
        /// </summary>
        Task RebuildTreeAsync(TreeIdentifier tree, CancellationToken cancellationToken = default);

        /// <summary>
        /// Rebuilds all trees for a given Standard Version (across all Scopes).
        /// Used as an admin/repair operation.
        /// </summary>
        Task RebuildAllTreesForStandardVersionAsync(
            Guid standardVersionId, CancellationToken cancellationToken = default);
    }
}
