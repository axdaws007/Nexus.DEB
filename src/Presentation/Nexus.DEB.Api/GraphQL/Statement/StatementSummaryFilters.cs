namespace Nexus.DEB.Api.GraphQL
{
    public class StatementSummaryFilters
    {
        public ICollection<Guid>? StandardVersionIds { get; set; }
        public ICollection<Guid>? ScopeIds { get; set; }
        public string? SearchText { get; set; }
        public DateOnly? ModifiedFrom { get; set; }
        public DateOnly? ModifiedTo { get; set; }
        public string? OwnedBy { get; set; }
        [ID]
        public ICollection<int?>? StatusIds { get; set; }
    }
}
