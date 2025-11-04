namespace Nexus.DEB.Application.Common.Models.Filters
{
    public class TaskSummaryFilters
    {
        public ICollection<Guid>? StandardVersionIds { get; set; }
        public string? SearchText { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public ICollection<short>? TaskTypeIds { get; set; }
        public ICollection<Guid>? OwnedById { get; set; }
        public ICollection<int?>? StatusIds { get; set; }
    }
}
