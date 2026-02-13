using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Sorting;

namespace Nexus.DEB.Application.Common.Models.Filters
{
    public class ScopeFilters : ISortableFilters
    {
        public ICollection<SortByItem>? SortBy { get; set; }
        public ICollection<Guid>? StandardVersionIds { get; set; }
        public ICollection<int?>? StatusIds { get; set; }

    }
}
