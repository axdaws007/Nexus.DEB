namespace Nexus.DEB.Api.GraphQL
{
    public class TaskSummaryFilters
    {
        public ICollection<Guid>? StandardVersionIds { get; set; }
        public string? SearchText { get; set; }
        public DateOnly? DueDateFrom { get; set; }
        public DateOnly? DueDateTo { get; set; }

        [ID]
        public ICollection<short>? TaskTypeIds { get; set; }

        public string? OwnedBy { get; set; }

        [ID]
        public ICollection<int?>? StatusIds { get; set; }
        public Guid? StatementId { get; set; }
    }
}
