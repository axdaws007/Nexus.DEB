using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Domain.Models
{
	public class StandardVersionDetailView
	{
		public Guid EntityId { get; set; }
		public string EntityTypeTitle { get; set; } = string.Empty;
		public string? SerialNumber { get; set; }
		public short StandardId { get; set; }
		public string StandardTitle { get; set; } = string.Empty;
		public string Delimiter { get; set; } = string.Empty;
		public string VersionTitle { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string? Description { get; set; }
		public DateTime CreatedDate { get; set; }
		public string CreatedBy { get; set; } = string.Empty;
		public Guid OwnedById { get; set; }
		public string OwnedBy { get; set; } = string.Empty;
		public string LastModifiedBy { get; set; } = string.Empty;
		public DateTime LastModifiedDate { get; set; }
		public int MajorVersion { get; set; }
		public int MinorVersion { get; set; }
		public DateOnly EffectiveStartDate { get; set; }
		public DateOnly? EffectiveEndDate { get; set; }
	}
}
