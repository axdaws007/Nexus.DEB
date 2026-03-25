using Nexus.DEB.Domain.Models.Enums;

namespace Nexus.DEB.Domain.Models.Other
{
    public class ComplianceTreeRebuildRequest
    {
        public Guid StandardVersionID { get; set; }
        public Guid ScopeID { get; set; }
        public ComplianceTreeRebuildStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public Guid? BuildId { get; set; }
        public DateTime? StartedAt { get; set; }
    }
}
