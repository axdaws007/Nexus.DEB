namespace Nexus.DEB.Application.Common.Models
{
    public class DestinationActivity
    {
        public int StepID { get; set; }
        public int TriggerStatusID { get; set; }
        public string CurrentPostTitle { get; set; }
        public bool IsCommentRequired { get; set; }
        public bool ShowSignoffText { get; set; }
        public string? SignoffText { get; set; }
        public bool RequirePassword { get; set; }
        public ICollection<TargetActivity>? TargetActivities { get; set; }
    }
}
