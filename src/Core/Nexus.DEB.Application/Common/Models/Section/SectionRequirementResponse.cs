namespace Nexus.DEB.Application.Common.Models
{
    public class SectionRequirementResponse
    {
        public Guid SectionId { get; set; }
        public IReadOnlyList<Guid> RequirementIds { get; set; } = [];

        public StandardVersionDetail? StandardVersion { get; set; }
    }
}
