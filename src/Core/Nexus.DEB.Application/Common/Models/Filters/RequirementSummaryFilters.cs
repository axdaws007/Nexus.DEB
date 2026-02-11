using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Sorting;

namespace Nexus.DEB.Application.Common.Models.Filters
{
    public class RequirementSummaryFilters : ISortableFilters
    {
        public ICollection<Guid>? StandardVersionIds { get; set; }
        public ICollection<Guid>? ScopeIds { get; set; }
        public string? SearchText { get; set; }
        public DateOnly? ModifiedFrom { get; set; }
        public DateOnly? ModifiedTo { get; set; }
        public ICollection<int?>? StatusIds { get; set; }
        public bool OnlyShowAvailableRequirementScopeCombinations { get; set; }
        public ICollection<SortByItem>? SortBy { get; set; }
    }
}
