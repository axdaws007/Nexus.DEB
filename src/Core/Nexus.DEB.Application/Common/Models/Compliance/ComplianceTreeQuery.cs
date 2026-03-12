using Nexus.DEB.Application.Common.Models.Compliance;

namespace Nexus.DEB.Application.Common.Models
{
    public class ComplianceTreeQuery
    {
        public required TreeIdentifier Tree { get; init; }

        /// <summary>
        /// Optional filter. Empty or null = return all nodes.
        /// When populated, only nodes matching these compliance state IDs
        /// are shown, plus their ancestors to preserve tree structure.
        /// </summary>
        public IReadOnlyList<int>? ComplianceStateFilter { get; init; }

        /// <summary>
        /// When true, Sections that contain no scoped Requirements
        /// (and whose descendant Sections also contain none) are excluded.
        /// Default: false (show all Sections).
        /// </summary>
        public bool HideEmptySections { get; init; }
    }
}
