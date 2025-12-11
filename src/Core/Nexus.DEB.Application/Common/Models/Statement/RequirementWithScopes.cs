namespace Nexus.DEB.Application.Common.Models
{
    public class RequirementWithScopes
    {
        public Guid RequirementId { get; set; }
        public string? SerialNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string StandardVersionReference { get; set; } = string.Empty;
        public List<ScopeCondensed> Scopes { get; set; } = new();
    }
}
