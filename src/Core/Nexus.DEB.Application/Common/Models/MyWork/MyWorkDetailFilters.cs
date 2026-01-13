namespace Nexus.DEB.Application.Common.Models
{
    public class MyWorkDetailFilters
    {
        public string RequiringProgressionBy { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string OwnedBy { get; set; } = string.Empty;
        public ICollection<Guid> MyTeamPostIds { get; set; } = [];
        public ICollection<Guid> ResponsibleGroupIds { get; set; } = [];
        public string EntityTypeTitle { get; set; } = string.Empty;
        public Guid? SelectedPostId { get; set; }
        public ICollection<int> ActivityIds { get; set; } = [];
        public DateTime? CreatedDateFrom { get; set; }
        public DateTime? CreatedDateTo { get; set; }
        public DateTime? AssignedDateFrom { get; set; }
        public DateTime? AssignedDateTo { get; set; }
    }
}
