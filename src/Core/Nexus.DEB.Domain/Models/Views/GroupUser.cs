namespace Nexus.DEB.Domain.Models
{
    public class GroupUser
    {
        public Guid EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
        public string? Email { get; set; }
        public bool? IsEnabled { get; set; }
        public bool IsDeleted { get; set; }
    }
}
