using Nexus.DEB.Application.Common.Models.Core;

namespace Nexus.DEB.Application.Common.Models
{
    public class StatementDetail : EntityDetailBase
    {
        public string StatementText { get; set; } = string.Empty;
        public DateTime? ReviewDate { get; set; }

        public ICollection<RequirementScopeDetail> RequirementScopeCombinations { get; set; }
    }
}
