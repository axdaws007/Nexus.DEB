using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Models
{
    public class ComplianceTreeResult
    {
        /// <summary>
        /// The filtered (or unfiltered) set of tree nodes.
        /// </summary>
        public IReadOnlyList<ComplianceTreeNodeResult> Nodes { get; init; }
            = Array.Empty<ComplianceTreeNodeResult>();

        /// <summary>
        /// All active compliance states (for legend/key rendering).
        /// </summary>
        public IReadOnlyList<ComplianceState> ComplianceStates { get; init; }
            = Array.Empty<ComplianceState>();

        /// <summary>
        /// Whether a filter was applied.
        /// </summary>
        public bool IsFiltered { get; init; }

        /// <summary>
        /// Whether empty sections are hidden
        /// </summary>
        public bool EmptySectionsHidden { get; init; }
    }
}
