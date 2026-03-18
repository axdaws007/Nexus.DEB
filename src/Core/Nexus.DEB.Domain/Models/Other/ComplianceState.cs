namespace Nexus.DEB.Domain.Models
{
    public class ComplianceState
    {
        public int ComplianceStateID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public string? Colour { get; set; }
        public bool IsTerminal { get; set; }
        public bool IsActive { get; set; }
    }
}
