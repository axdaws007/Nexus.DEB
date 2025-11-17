using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Models
{
    public class TargetActivity
    {
        public string DestinationActivityTitle { get; set; } = string.Empty;
        public int DestinationActivityID { get; set; }
        public string SourceActivityTitle { get; set; } = string.Empty;
        public int SourceActivityID { get; set; }
        public OwnerRequired OwnerRequired { get; set; }
        public bool IsCommentRequired { get; set; }
        public TransitionType TransitionType { get; set; }

        public ICollection<string> PickerRoles { get; set; } = new List<string>();
        public ICollection<string> ValidatorTags { get; set; } = new List<string>();
        public ICollection<string> SideEffectTags { get; set; } = new List<string>();
    }
}
