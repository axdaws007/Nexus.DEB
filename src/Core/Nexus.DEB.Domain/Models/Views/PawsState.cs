namespace Nexus.DEB.Domain.Models
{
    public class PawsState
    {
        public Guid EntityId { get; set; }
        public int? StatusId { get; set; }
        public string? Status { get; set; }
        public Guid WorkflowId { get; set; }
    }
}
