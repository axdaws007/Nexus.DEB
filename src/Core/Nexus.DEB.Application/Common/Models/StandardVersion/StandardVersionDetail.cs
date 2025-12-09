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
		public List<Scope> Scopes { get; set; } = new();
	}
}
