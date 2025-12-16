namespace Nexus.DEB.Application.Common.Models
{
    public class CreateWorkflowInstanceRequest
    {
        public Guid WorkflowID { get; set; }
        public Guid EntityID { get; set; }
        public int? ActivityID { get; set; }
        public Guid? OwnerID { get; set; }
    }
}
