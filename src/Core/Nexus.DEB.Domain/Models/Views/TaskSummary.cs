using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Domain.Models
{
    public class TaskSummary : IEntityType, IOwnedBy
    {
        public Guid EntityId { get; set; }
        public string? SerialNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid OwnedById { get; set; }
        public string? OwnedBy { get; set; }
        public DateOnly? DueDate { get; set; }
        public DateOnly? OriginalDueDate { get; set; }
        public short TaskTypeId { get; set; }
        public string TaskTypeTitle { get; set; } = string.Empty;
        public int? StatusId { get; set; }
        public string? Status { get; set; }
        public Guid StatementId { get; set; }
        public string EntityTypeTitle { get; set; } = string.Empty;
    }
}
