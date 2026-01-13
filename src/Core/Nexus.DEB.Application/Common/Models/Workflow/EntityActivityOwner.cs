namespace Nexus.DEB.Application.Common.Models
{
    public enum ActivityOwnerType
    {
        None = 1,
        User,
        Role,
        Group
    }

    public class EntityActivityOwner
    {
        public int EntityActivityOwnerID { get; set; }

        public Guid EntityID { get; set; }

        public int ActivityID { get; set; }

        public Guid? OwnerID { get; set; }

        public ActivityOwnerType OwnerType { get; set; }
    }
}
