using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Domain.Models
{
    public class TaskSummary : IEntity, IOwnedBy
    {
        public Guid EntityId { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public Guid OwnedById { get; set; }
        public DateTime? DueDate { get; set; }
        public string TaskTypeTitle { get; set; } = string.Empty;
        public int? StatusId { get; set; }
        public string? Status { get; set; }
    }
}
