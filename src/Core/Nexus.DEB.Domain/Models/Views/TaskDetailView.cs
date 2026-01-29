namespace Nexus.DEB.Domain.Models
{
	public class TaskDetailView : EntityDetailViewBase
	{
		public DateOnly? DueDate { get; set; }
        public DateOnly? OriginalDueDate { get; set; }
        public short TaskTypeId { get; set; }
		public string TaskType { get; set; } = string.Empty;
		public int? ActivityId { get; set; }
		public string Status { get; set; } = string.Empty;
		public Guid StatementId { get; set; }
		public string StatementTitle { get; set; } = string.Empty;
		public string StatementSerialNumber { get; set; } = string.Empty;
	}
}
