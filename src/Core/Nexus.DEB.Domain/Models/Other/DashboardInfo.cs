namespace Nexus.DEB.Domain.Models.Other
{
    public class DashboardInfo
    {
        public Guid EntityId { get; set; }
        public DateOnly? DueDate { get; set; }
        public bool IsOpen { get; set; }
        public Guid? AssignedToPostId { get; set; }
        public DateTime? EntityOpenDate { get; set; }
        public DateTime? EntityClosedDate { get; set; }
        public bool IsWorkflowActive { get; set; }
        public DateOnly? ReviewDate { get; set; }
        public Guid? ResponsibleOwnerId { get; set; }
    }
}
