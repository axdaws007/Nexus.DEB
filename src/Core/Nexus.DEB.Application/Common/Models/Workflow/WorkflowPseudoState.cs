namespace Nexus.DEB.Application.Common.Models
{
    public class WorkflowPseudoState
    {
        public int PseudoStateID { get; set; }
        public string PseudoStateTitle { get; set; }
        public string? PseudoStateDescription { get; set; }
        public Guid ProcessTemplateID { get; set; }
    }
}
