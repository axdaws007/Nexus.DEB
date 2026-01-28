using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Models
{
	public class StandardVersionWithSections
	{
		public Guid EntityId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string? SerialNumber { get; set; }

		public ICollection<Section> Sections { get; set; } = new List<Section>();
	}
}
