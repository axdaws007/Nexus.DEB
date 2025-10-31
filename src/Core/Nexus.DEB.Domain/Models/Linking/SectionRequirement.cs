namespace Nexus.DEB.Domain.Models
{
    public class SectionRequirement
    {
        public Guid SectionID { get; set; }
        public virtual Section Section { get; set; }
        public Guid RequirementID { get; set; }
        public virtual Requirement Requirement { get; set; }
        public int Ordinal { get; set; }
        public bool IsEnabled { get; set; } = true;

        public DateTime LastModifiedAt { get; set; }
        public Guid LastModifiedBy { get; set; }
    }
}
