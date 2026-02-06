using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Domain.Models.Views
{
	public class ScopeDetailView
	{
		public Guid EntityId { get; set; }
		public string EntityTypeTitle { get; set; } = string.Empty;
		public string? SerialNumber { get; set; }
		public string Title { get; set; } = string.Empty;
		public string? Description { get; set; }
		public DateTime CreatedDate { get; set; }
		public string CreatedBy { get; set; } = string.Empty;
		public Guid OwnedById { get; set; }
		public string OwnedBy { get; set; } = string.Empty;
		public string LastModifiedBy { get; set; } = string.Empty;
		public DateTime LastModifiedDate { get; set; }
		public DateOnly? TargetImplementationDate { get; set; }
		public bool IsRemoved { get; set; }
		public bool IsArchived { get; set; }
	}
}
