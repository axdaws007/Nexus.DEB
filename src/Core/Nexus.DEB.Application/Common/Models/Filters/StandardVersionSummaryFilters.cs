using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Sorting;

namespace Nexus.DEB.Application.Common.Models.Filters
{
    public class StandardVersionSummaryFilters : ISortableFilters
    {
        public ICollection<short>? StandardIds { get; set; }
        public ICollection<int>? StatusIds { get; set; }
        public DateOnly? EffectiveFromDate { get; set; }
        public DateOnly? EffectiveToDate { get; set; }
        public ICollection<SortByItem>? SortBy { get; set; }
    }
}
