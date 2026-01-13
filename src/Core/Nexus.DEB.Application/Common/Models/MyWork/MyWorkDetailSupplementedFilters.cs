namespace Nexus.DEB.Application.Common.Models
{
    public class MyWorkDetailSupplementedFilters : MyWorkDetailFilters
    {
        public Guid PostId { get; set; }
        public ICollection<Guid> RoleIds { get; set; }
        public Guid WorkflowId { get; set; }

    }
}
