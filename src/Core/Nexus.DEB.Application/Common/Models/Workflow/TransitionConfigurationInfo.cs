namespace Nexus.DEB.Application.Common.Models
{
    public class TransitionConfigurationInfo
    {
        public int ActivityTransitionId { get; set; }
        public int SourceActivityId { get; set; }
        public int DestinationActivityId { get; set; }
        public int TriggerStatusId { get; set; }

        // Parse MUTTags column into list of validator names
        public List<string> ValidatorNames { get; set; } = new();

        // Parse MUTHandler column into list of side effect names
        public List<string> SideEffectNames { get; set; } = new();
    }
}
