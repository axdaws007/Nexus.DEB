namespace Nexus.DEB.Application.Common.Models
{
    public class TargetActivity
    {
        public string DestinationActivityTitle { get; set; } = string.Empty;
        public int DestinationActivityID { get; set; }
        public string SourceActivityTitle { get; set; } = string.Empty;
        public int SourceActivityID { get; set; }
        public string OwnerRequired { get; set; } = string.Empty;
        public ICollection<string> PickerRoles { get; set; } = new List<string>();
        public ICollection<string> ValidatorTags { get; set; } = new List<string>();
        public ICollection<string> SideEffectTags { get; set; } = new List<string>();
    }
}
