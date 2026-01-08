namespace Nexus.DEB.Application.Common.Models
{
    public class MyWorkSummaryFilters
    {
        public string RequiringProgressionBy { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string OwnedBy { get; set; } = string.Empty;
        public ICollection<Guid> MyTeamPostIds { get; set; } = [];
        public ICollection<Guid> ResponsibleGroupIds { get; set; } = [];
    }
}
