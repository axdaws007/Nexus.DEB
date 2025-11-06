using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class TaskSummaryFilters
    {
        public ICollection<Guid>? StandardVersionIds { get; set; }
        public string? SearchText { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }

        [ID(nameof(Standard))]
        public ICollection<short>? TaskTypeIds { get; set; }

        public string? OwnedBy { get; set; }

        [ID]
        public ICollection<int?>? StatusIds { get; set; }
        public Guid? StatementId { get; set; }
    }
}
