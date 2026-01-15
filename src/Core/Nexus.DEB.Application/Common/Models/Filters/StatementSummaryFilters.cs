

namespace Nexus.DEB.Application.Common.Models
{
    public class StatementSummaryFilters
    {
        public ICollection<Guid>? StandardVersionIds { get; set; }
        public ICollection<Guid>? ScopeIds { get; set; }
        public string? SearchText { get; set; }
        public DateOnly? ModifiedFrom { get; set; }
        public DateOnly? ModifiedTo { get; set; }
        public ICollection<Guid>? OwnedByIds { get; set; }
        public ICollection<int?>? StatusIds { get; set; }
    }
}
