namespace Nexus.DEB.Application.Common.Models.Filters
{
    public class TaskSummaryFilters
    {
        public ICollection<Guid>? StandardVersionIds { get; set; }
        public string? SearchText { get; set; }
        public DateOnly? DueDateFrom { get; set; }
        public DateOnly? DueDateTo { get; set; }
        public ICollection<short>? TaskTypeIds { get; set; }
        public ICollection<Guid>? OwnedByIds { get; set; }
        public ICollection<int?>? StatusIds { get; set; }
        public Guid? StatementId { get; set; }
    }
}
