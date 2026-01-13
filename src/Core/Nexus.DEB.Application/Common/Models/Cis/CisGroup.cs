namespace Nexus.DEB.Application.Common.Models
{
    public enum GroupType
    {
        AccessItemGroup,
        PostGroup,
        UserGroup
    }

    public class CisGroup
    {
        public Guid ID { get; set; }
        public Guid? ParentID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public GroupType GroupType { get; set; }
        public ICollection<CisGroupApplicability> GroupApplicabilities { get; set; } = [];
    }
}
