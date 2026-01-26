using Nexus.DEB.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Application.Common.Models.StandardVersion
{
	public class StandardVersionWithSections
	{
		public Guid EntityId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string? SerialNumber { get; set; }

		public List<Section> Sections { get; set; } = new List<Section>();
	}
}
