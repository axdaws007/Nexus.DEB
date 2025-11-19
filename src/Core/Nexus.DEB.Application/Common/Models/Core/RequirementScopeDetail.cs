namespace Nexus.DEB.Application.Common.Models
{
    public class RequirementScopeDetail
    {
        public Guid RequirementId { get; set; }
        public string? RequirementSerialNumber { get; set; }
        public string RequirementTitle { get; set; } = string.Empty;
        public Guid ScopeId { get; set; }
        public string? ScopeSerialNumber { get; set; }
        public string ScopeTitle { get; set; } = string.Empty;
    }
}