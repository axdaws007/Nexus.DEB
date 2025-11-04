namespace Nexus.DEB.Application.Common.Models.Filters
{
    public class StandardVersionSummaryFilters
    {
        public ICollection<short>? StandardIds { get; set; }
        public ICollection<int>? StatusIds { get; set; }
        public DateTime? EffectiveFromDate { get; set; }
        public DateTime? EffectiveToDate { get; set; }
    }
}
