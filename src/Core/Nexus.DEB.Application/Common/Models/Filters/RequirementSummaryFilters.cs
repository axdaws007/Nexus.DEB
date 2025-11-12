namespace Nexus.DEB.Application.Common.Models.Filters
{
    public class RequirementSummaryFilters
    {
        public ICollection<Guid>? StandardVersionIds { get; set; }
        public ICollection<Guid>? ScopeIds { get; set; }
        public string? SearchText { get; set; }
        public DateTime? ModifiedFrom { get; set; }
        public DateTime? ModifiedTo { get; set; }
        public ICollection<int?>? StatusIds { get; set; }
        public Guid? StatementId { get; set; }
    }
}
