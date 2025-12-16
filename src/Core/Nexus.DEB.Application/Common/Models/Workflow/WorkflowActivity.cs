namespace Nexus.DEB.Application.Common.Models
{
    public class WorkflowActivity
    {
        public int ActivityID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid ProcessTemplateID { get; set; }
        public List<Guid> OwnerRoleIDs { get; set; } = new();
        public string SignoffText { get; set; } = string.Empty;
        public bool ShowSignoffText { get; set; }
        public bool RequirePassword { get; set; }
        public bool IsRemoved { get; set; }
    }
}
