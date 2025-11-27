namespace Nexus.DEB.Application.Common.Models
{
    public class RequirementScopes
    {
        public Guid RequirementId { get; set; }
        public ICollection<Guid> ScopeIds { get; set; } = [];
    }
}
