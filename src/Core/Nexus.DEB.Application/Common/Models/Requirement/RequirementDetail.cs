using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Application.Common.Models
{
	public class RequirementDetail : EntityDetailBase
	{
		public int RequirementTypeId { get; set; }
		public string RequirementTypeTitle { get; set; } = string.Empty;
		public int RequirementCategoryId { get; set; }
		public string RequirementCategoryTitle { get; set; } = string.Empty;
	}
}
