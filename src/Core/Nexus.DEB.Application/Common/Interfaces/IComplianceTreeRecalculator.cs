using Nexus.DEB.Application.Common.Models.Compliance;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IComplianceTreeRecalculator
    {
        /// <summary>
        /// Recalculates a single entity's compliance state (from its pseudostate)
        /// and bubbles up to the root in every tree containing this entity.
        /// Operates on the live build's nodes.
        /// </summary>
        Task RecalculateFromEntityAsync(
            Guid entityId, string entityType, string nodeType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Recalculates a parent node from its children within a specific tree,
        /// then bubbles up to root. Operates on the live build's nodes.
        /// </summary>
        Task RecalculateFromParentAsync(
            TreeIdentifier tree, Guid parentEntityId, string parentNodeType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Full rebuild of a specific compliance tree (Standard Version + Scope).
        /// Writes all nodes against the specified BuildId.
        /// Calls checkpointCallback at level boundaries — if it returns false,
        /// the rebuild should be considered abandoned (caller handles cleanup).
        /// </summary>
        Task RebuildTreeAsync(
            TreeIdentifier tree,
            Guid buildId,
            Func<Task<bool>>? checkpointCallback = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Convenience overload for ad-hoc/admin rebuilds that go straight
        /// to live without the managed background process.
        /// </summary>
        Task RebuildTreeDirectAsync(
            TreeIdentifier tree, CancellationToken cancellationToken = default);

        /// <summary>
        /// Rebuilds all trees for a given Standard Version (across all Scopes).
        /// Each tree is rebuilt directly (not via the background queue).
        /// </summary>
        Task RebuildAllTreesForStandardVersionDirectAsync(
            Guid standardVersionId, CancellationToken cancellationToken = default);
    }
}
