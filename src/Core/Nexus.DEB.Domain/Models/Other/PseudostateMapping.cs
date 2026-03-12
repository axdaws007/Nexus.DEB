namespace Nexus.DEB.Domain.Models
{
    public class PseudostateMapping
    {
        public int PseudostateMappingID { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int PseudoStateID { get; set; }
        public string PseudoStateTitle { get; set; } = string.Empty;
        public int ComplianceStateID { get; set; }
        public virtual ComplianceState ComplianceState { get; set; } = null!;
    }
}
