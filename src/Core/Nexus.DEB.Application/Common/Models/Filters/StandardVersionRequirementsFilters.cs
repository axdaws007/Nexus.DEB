using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Application.Common.Models
{
	public class StandardVersionRequirementsFilters
	{
		public Guid? StandardVersionId { get; set; }
		public Guid? SectionId { get; set; }
		public string? SearchText { get; set; }
		public Guid ScopeId { get; set; }
	}
}
