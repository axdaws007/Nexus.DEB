

namespace Nexus.DEB.Application.Common.Models
{
    public class StatementSummaryFilters
    {
        public Guid? StandardVersionId { get; set; }
        public Guid? ScopeId { get; set; }
        public string? SearchText { get; set; }
        public DateTime? ModifiedFrom { get; set; }
        public DateTime? ModifiedTo { get; set; }
        public Guid? OwnedById { get; set; }
    }
}
