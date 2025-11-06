namespace Nexus.DEB.Application.Common.Models
{
    public class PendingActivity
    {
        public int StepID { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ActivityID { get; set; }
        public int PseudoStateID { get; set; }
        public string PseudoStateTitle { get; set; } = string.Empty;
        public ICollection<TriggerStates>? AvailableTriggerStates { get; set; }
    }
}
