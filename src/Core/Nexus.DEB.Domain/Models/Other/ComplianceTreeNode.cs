namespace Nexus.DEB.Domain.Models
{
    public class ComplianceTreeNode
    {
        public long ComplianceTreeNodeID { get; set; }
        public Guid StandardVersionID { get; set; }
        public Guid ScopeID { get; set; }
        public string NodeType { get; set; } = string.Empty;
        public Guid EntityID { get; set; }
        public string? ParentNodeType { get; set; }
        public Guid? ParentEntityID { get; set; }
        public int? ComplianceStateID { get; set; }
        public string? ComplianceStateLabel { get; set; }
        public int? PseudoStateID { get; set; }
        public int? ActivityId { get; set; }
        public int? StatusId { get; set; }
        public int? TotalRequirementCount { get; set; }
        public int? TotalSectionCount { get; set; }
        public DateTime LastCalculatedAt { get; set; }
        public virtual ComplianceState? ComplianceState { get; set; }
        public virtual ICollection<ComplianceTreeNodeSummary> Summaries { get; set; } = [];
    }
}
