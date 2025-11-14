namespace Nexus.DEB.Domain.Models
{
    public class PawsEntityDetail
    {
        public Guid EntityId { get; set; }
        public int StepId { get; set; }
        public int ActivityId { get; set; }
        public string ActivityTitle { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string StatusTitle { get; set; } = string.Empty;
        public int? PseudoStateId { get; set; }
        public string? PseudoStateTitle { get; set; }
    }
}
