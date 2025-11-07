namespace Nexus.DEB.Domain.Models
{
    public class TaskExport
    {
        public Guid EntityId { get; set; }
        public string? SerialNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid OwnedById { get; set; }
        public string? OwnedBy { get; set; }
        public DateTime? DueDate { get; set; }
        public string TaskTypeTitle { get; set; } = string.Empty;
        public int? StatusId { get; set; }
        public string? Status { get; set; }
        public Guid StatementId { get; set; }
        public string? StatementSerialNumber { get; set; }
    }
}
