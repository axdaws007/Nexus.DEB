namespace Nexus.DEB.Domain.Models
{
    public class StatementDetailView : EntityDetailViewBase
    {
        public string StatementText { get; set; } = string.Empty;
        public DateTime? ReviewDate { get; set; }
        public Guid ScopeID { get; set; }
        public string? Scope { get; set; }
    }
}
