namespace Nexus.DEB.Application.Common.Models.Filters
{
    public class StandardVersionSummaryFilters
    {
        public ICollection<short>? StandardIds { get; set; }
        public ICollection<int>? StatusIds { get; set; }
        public DateOnly? EffectiveFromDate { get; set; }
        public DateOnly? EffectiveToDate { get; set; }
    }
}
