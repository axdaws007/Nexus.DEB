namespace Nexus.DEB.Application.Common.Models.Filters
{
    public class StandardVersionSummaryFilters
    {
        public short? StandardId { get; set; }
        public int? StatusId { get; set; }
        public DateTime? EffectiveFromDate { get; set; }
        public DateTime? EffectiveToDate { get; set; }
    }
}
