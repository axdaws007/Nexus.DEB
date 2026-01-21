using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Domain.Models
{
    public class Scope : EntityHead
    {
        public DateOnly? TargetImplementationDate { get; set; }
        public virtual ICollection<Requirement> Requirements { get; set; }
        public virtual ICollection<StatementRequirementScope> StatementsRequirementsScopes { get; set; }

    }
}
