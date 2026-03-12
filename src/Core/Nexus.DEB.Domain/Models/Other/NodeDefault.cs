namespace Nexus.DEB.Domain.Models
{
    public class NodeDefault
    {
        public int NodeDefaultID { get; set; }
        public string NodeType { get; set; } = string.Empty;
        public string Scenario { get; set; } = string.Empty;
        public int? DefaultComplianceStateID { get; set; }
        public string? DefaultLabel { get; set; }
        public virtual ComplianceState? DefaultComplianceState { get; set; }
    }
}
