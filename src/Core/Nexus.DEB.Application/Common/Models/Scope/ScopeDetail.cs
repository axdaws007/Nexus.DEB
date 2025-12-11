using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Application.Common.Models
{
	public class ScopeDetail : EntityDetailBase
	{
		public DateTime? TargetImplementationDate { get; set; }

		public List<StandardVersionRequirements> StandardVersionRequirements { get; set; } = new();
	}
}
