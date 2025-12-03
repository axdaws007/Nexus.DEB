using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Domain.Models
{
	public class ChangeRecordItem
	{
		public int Id { get; set; }
		public int ChangeRecordId { get; set; }
		public string FieldName { get; set; } = string.Empty;
		public string FriendlyFieldName { get; set; } = string.Empty;
		public string? ChangedFrom { get; set; } = string.Empty;
		public string? ChangedTo { get; set; } = string.Empty;
		public bool IsDeleted { get; set; } = false;

		public virtual ChangeRecord ChangeRecord { get; set; }
	}
}
