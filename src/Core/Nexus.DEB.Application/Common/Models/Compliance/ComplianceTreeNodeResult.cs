using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Models
{
    public class ComplianceTreeNodeResult
    {
        public required ComplianceTreeNode Node { get; init; }

        /// <summary>
        /// True if this node directly matched the compliance state filter.
        /// False if this node is included only to preserve the tree path
        /// to a matching descendant. Always true when no filter is applied.
        /// </summary>
        public bool IsDirectMatch { get; init; } = true;

        public long? ParentComplianceTreeNodeID { get; init; }
    }
}
