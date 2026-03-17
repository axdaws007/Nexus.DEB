namespace Nexus.DEB.Domain.Models
{
    public class BubbleUpRule
    {
        public int BubbleUpRuleID { get; set; }
        public string ParentNodeType { get; set; } = string.Empty;
        public int Ordinal { get; set; }
        public string Quantifier { get; set; } = string.Empty;
        public int ChildComplianceStateID { get; set; }
        public int ResultComplianceStateID { get; set; }
        public bool IsActive { get; set; }
        public virtual ComplianceState ChildComplianceState { get; set; } = null!;
        public virtual ComplianceState ResultComplianceState { get; set; } = null!;
    }
}
