namespace Nexus.DEB.Application.Common.Models
{
    public class SectionRequirementResponse
    {
        public Guid SectionId { get; set; }
        public ICollection<RequirementItem> Requirements { get; set; } = [];

        public StandardVersionDetail? StandardVersion { get; set; }
    }
}
