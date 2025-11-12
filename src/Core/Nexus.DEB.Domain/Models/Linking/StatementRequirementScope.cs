namespace Nexus.DEB.Domain.Models
{
    public class StatementRequirementScope
    {
        public Guid StatementId { get; set; }
        public virtual Statement Statement { get; set; }
        public Guid RequirementId { get; set; }
        public virtual Requirement Requirement { get; set; }
        public Guid ScopeId { get; set; }
        public virtual Scope Scope { get; set; }
    }
}
