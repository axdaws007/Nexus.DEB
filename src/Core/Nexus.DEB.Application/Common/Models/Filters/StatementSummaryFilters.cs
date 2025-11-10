

namespace Nexus.DEB.Application.Common.Models
{
    public class StatementSummaryFilters
    {
        public ICollection<Guid>? StandardVersionIds { get; set; }
        public ICollection<Guid>? ScopeIds { get; set; }
        public string? SearchText { get; set; }
        public DateTime? ModifiedFrom { get; set; }
        public DateTime? ModifiedTo { get; set; }
        public ICollection<Guid>? OwnedByIds { get; set; }
        public ICollection<int?>? StatusIds { get; set; }
    }
}
