namespace Nexus.DEB.Domain.Models
{
    public class ComplianceStateMapping
    {
        public int ComplianceStateMappingID { get; set; }
        public Guid WorkflowID { get; set; }
        public int ActivityID { get; set; }
        public string ActivityTitle { get; set; } = string.Empty;
        public int StatusID { get; set; }
        public string StatusTitle { get; set; } = string.Empty;
        public int ComplianceStateID { get; set; }
        public virtual ComplianceState ComplianceState { get; set; } = null!;
    }
}
