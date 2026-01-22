using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Domain.Models
{
	public class StandardVersionRequirementDetail
	{
		public Guid RequirementId { get; set; }
		public string SerialNumber { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public Guid? StandardVersionId { get; set; }
		public string StandardVersion { get; set; }
		public Guid? SectionId { get; set; }
		public string Section { get; set; }
		public int OtherScopes { get; set; } = 0;
		public bool IncludedInScope { get; set; } = false;
	}
}
