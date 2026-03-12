using Nexus.DEB.Application.Common.Models.Compliance;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IComplianceStateEngine
    {
        /// <summary>
        /// Resolves an entity's intrinsic compliance state from its pseudostate.
        /// </summary>
        Task<int?> ResolveComplianceStateAsync(WorkflowInfo workflowInfo);

        /// <summary>
        /// Evaluates bubble-up rules for a parent node given its children's compliance states.
        /// Rules are processed in ordinal order; first match wins.
        /// </summary>
        Task<ComplianceStateResult> EvaluateBubbleUpAsync(
            string parentNodeType,
            IReadOnlyCollection<int?> childComplianceStateIds);

        /// <summary>
        /// Calculates aggregate counts of descendant Requirements by compliance state.
        /// Used by Sections and the root node. Recursive through child Sections.
        /// </summary>
        Task<IReadOnlyList<ComplianceTreeNodeSummary>> CalculateRequirementAggregatesAsync(
            TreeIdentifier tree,
            Guid parentEntityId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Calculates aggregate counts of direct child Sections by compliance state.
        /// Used by the root node only.
        /// </summary>
        Task<IReadOnlyList<ComplianceTreeNodeSummary>> CalculateSectionAggregatesAsync(
            TreeIdentifier tree,
            Guid rootEntityId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all active compliance states (for UI rendering).
        /// Returns cached data.
        /// </summary>
        Task<IReadOnlyList<ComplianceState>> GetActiveComplianceStatesAsync();
    }
}
