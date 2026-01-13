namespace Nexus.DEB.Application.Common.Models
{
    public class WorkflowAuditData
    {
        public CurrentWorkflowStatus CurrentWorkflowStatus { get; set; }
        public EntityActivityStep? LastSignedStep { get; set; }
    }
}
