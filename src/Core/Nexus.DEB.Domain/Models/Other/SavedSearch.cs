using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Domain.Models
{
	public class SavedSearch
	{
		public Guid PostId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Context { get; set; } = string.Empty;
		public string? Filter { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime? LastModifiedDate { get; set; }
	}
}
