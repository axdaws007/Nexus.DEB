namespace Nexus.DEB.Application.Common.Models
{
    public class WorkflowHistory
    {
        public bool EnableUndo { get; set; }
        public bool ShowActivityOwner { get; set; }
        public ICollection<WorkflowHistoryItem>? Items { get; set; }
    }
}
