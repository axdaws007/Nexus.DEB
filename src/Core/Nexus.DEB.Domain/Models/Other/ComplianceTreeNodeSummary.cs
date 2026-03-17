namespace Nexus.DEB.Domain.Models
{
    public class ComplianceTreeNodeSummary
    {
        public long ComplianceTreeNodeID { get; set; }
        public string ChildNodeType { get; set; } = string.Empty;
        public int ComplianceStateID { get; set; }
        public int Count { get; set; }
        public virtual ComplianceTreeNode TreeNode { get; set; } = null!;
        public virtual ComplianceState ComplianceState { get; set; } = null!;
    }
}
