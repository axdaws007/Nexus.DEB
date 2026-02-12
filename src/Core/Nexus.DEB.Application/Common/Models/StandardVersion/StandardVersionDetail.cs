using Nexus.DEB.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Application.Common.Models
{
	public class StandardVersionDetail : EntityDetailBase
	{
		public short StandardId { get; set; }
		public string StandardTitle { get; set; } = string.Empty;
		public string Delimiter { get; set; } = string.Empty;
		public string VersionTitle { get; set; } = string.Empty;
		public int? MajorVersion { get; set; }
		public int? MinorVersion { get; set; }
		public DateOnly EffectiveStartDate { get; set; }
		public DateOnly? EffectiveEndDate { get; set; }
		public List<Scope> Scopes { get; set; } = new();
	}
}
