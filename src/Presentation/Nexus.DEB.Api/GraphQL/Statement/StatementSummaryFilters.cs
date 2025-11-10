namespace Nexus.DEB.Api.GraphQL
{
    public class StatementSummaryFilters
    {
        public ICollection<Guid>? StandardVersionIds { get; set; }
        public ICollection<Guid>? ScopeIds { get; set; }
        public string? SearchText { get; set; }
        public DateTime? ModifiedFrom { get; set; }
        public DateTime? ModifiedTo { get; set; }
        public string? OwnedBy { get; set; }
        [ID]
        public ICollection<int?>? StatusIds { get; set; }
    }
}
