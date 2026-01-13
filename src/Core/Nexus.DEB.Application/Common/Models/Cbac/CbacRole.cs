namespace Nexus.DEB.Application.Common.Models
{
    public class CbacRole
    {
        public Guid RoleID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? BusinessDescription { get; set; }
    }
}
