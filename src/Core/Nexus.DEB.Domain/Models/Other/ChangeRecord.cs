using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Domain.Models
{
	public class ChangeRecord
	{
		public int Id { get; set; }
		public Guid EntityId { get; set; }
		public DateTime ChangeDate { get; set; }
		public string? ChangeByUser { get; set; }
		public string? Comments { get; set; }
		public bool IsDeleted { get; set; } = false;
		public Guid EventId { get; set; }

		public virtual ICollection<ChangeRecordItem> ChangeRecordItems { get; set; }
	}
}
