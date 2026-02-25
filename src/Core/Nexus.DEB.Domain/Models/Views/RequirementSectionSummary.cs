namespace Nexus.DEB.Domain.Models.Views
{
    public class RequirementSectionSummary
    {
        public Guid RequirementId { get; set; }
        public string? SerialNumber { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int StatusId { get; set; }
        public string? Status { get; set; }
        public int NumberOfLinkedSections { get; set; }
    }
}
