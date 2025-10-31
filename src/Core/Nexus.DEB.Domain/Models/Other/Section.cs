namespace Nexus.DEB.Domain.Models
{
    public class Section
    {
        public Guid Id { get; set; }
        public string? Reference { get; set; }
        public string? Title { get; set; }
        public bool IsReferenceDisplayed { get; set; }
        public bool IsTitleDisplayed { get; set; }
        public Guid? ParentSectionId { get; set; }
        public int Ordinal { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public virtual Section ParentSection { get; set; }

        public virtual List<Section> ChildSections { get; set; }

        public Guid StandardVersionId { get; set; }
        public virtual StandardVersion StandardVersion { get; set; }
        public virtual ICollection<SectionRequirement> SectionRequirements { get; set; }
    }
}
