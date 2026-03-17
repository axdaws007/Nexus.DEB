using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Models.Compliance
{
    public record TraversalEntry(ComplianceTreeNode Node, long? ParentComplianceTreeNodeID);
}
