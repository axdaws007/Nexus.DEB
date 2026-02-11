using Nexus.DEB.Application.Common.Models.Sorting;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ISortableFilters
    {
        ICollection<SortByItem>? SortBy { get; set; }
    }
}
