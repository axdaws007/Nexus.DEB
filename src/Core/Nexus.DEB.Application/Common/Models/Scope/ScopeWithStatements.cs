using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Models
{
	public class ScopeWithStatements
	{
		public Guid EntityId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string? SerialNumber { get; set; }

		public List<Statement> Statements { get; set; } = new List<Statement>();

	}
}
