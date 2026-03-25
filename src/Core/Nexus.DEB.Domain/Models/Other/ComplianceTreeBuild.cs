namespace Nexus.DEB.Domain.Models.Other
{
    public class ComplianceTreeBuild
    {
        public Guid StandardVersionID { get; set; }
        public Guid ScopeID { get; set; }
        public Guid LiveBuildId { get; set; }
        public DateTime PromotedAt { get; set; }
    }
}
