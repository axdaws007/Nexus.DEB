namespace Nexus.DEB.Application.Common.Models
{
	public class TaskDetail : EntityDetailBase
	{
		public DateOnly? DueDate { get; set; }
        public DateOnly? OriginalDueDate { get; set; }
        public int TaskTypeId { get; set; }
		public string TaskType { get; set; } = string.Empty;
		public int? ActivityId { get; set; }
		public string Status { get; set; }
		public Guid StatementId { get; set; }
		public string StatementTitle { get; set; } = string.Empty;
		public string StatementSerialNumber { get; set; } = string.Empty;
	}
}
