namespace Nexus.DEB.Application.Common.Models
{
    public class CurrentWorkflowStatus
    {
        public int ActivityId { get; set; }
        public string ActivityTitle { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string StatusTitle { get; set; } = string.Empty;
        public int? PseudoStateId { get; set; }
        public string? PseudoStateTitle { get; set; }
        public int StepId { get; set; }
        public ICollection<TriggerStates>? AvailableTriggerStates { get; set; } = new List<TriggerStates>();
    }
}
