using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Domain.Models
{
	public class TaskDetailView : EntityDetailViewBase
	{
		public DateTime? DueDate { get; set; }
		public short TaskTypeId { get; set; }
		public string TaskType { get; set; } = string.Empty;
		public int? ActivityId { get; set; }
		public string Status { get; set; }
		public Guid StatementId { get; set; }
		public string StatementTitle { get; set; } = string.Empty;
		public string StatementSerialNumber { get; set; } = string.Empty;
	}
}
