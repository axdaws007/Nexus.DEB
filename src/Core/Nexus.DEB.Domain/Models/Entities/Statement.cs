using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Domain.Models
{
    public class Statement : EntityHead
    {
        public string StatementText { get; set; }
        public DateTime? ReviewDate { get; set; }
        public Guid ScopeID { get; set; }
        public virtual Scope Scope { get; set; }

        public virtual ICollection<StatementRequirementScope> StatementsRequirementsScopes { get; set; }
        public virtual ICollection<Requirement> Requirements { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }
    }
}
