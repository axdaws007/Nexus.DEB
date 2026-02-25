namespace Nexus.DEB.Application.Common.Models
{
    public class RequirementSectionFilters
    {
        public ICollection<int?>? StatusIds { get; set; }
        public string? SearchText { get; set; }
        public bool? HasSectionsAssigned { get; set; }
    }
}
