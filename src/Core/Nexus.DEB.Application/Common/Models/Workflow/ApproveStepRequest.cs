namespace Nexus.DEB.Application.Common.Models
{
    public class ApproveStepRequest
    {
        public int StepID { get; set; }
        public Guid EntityID { get; set; }
        public Guid WorkflowID { get; set; }
        public int SelectedStateID { get; set; }
        public string Comments { get; set; } = string.Empty;
        public Guid? OnBehalfOfID { get; set; }
        public string Password { get; set; } = string.Empty;
        public Guid[] DefaultOwnerID { get; set; }
        public int[] DestinationActivityID { get; set; }
    }
}
