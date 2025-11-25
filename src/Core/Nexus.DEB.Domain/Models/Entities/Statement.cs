using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Domain.Models
{
    public class Statement : EntityHead
    {
        public DateTime? ReviewDate { get; set; }

        public virtual ICollection<StatementRequirementScope> StatementsRequirementsScopes { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }
    }
}
