namespace Nexus.DEB.Application.Common.Models
{
    public class CisUser
    {
        public Guid UserId { get; set; }
        public List<CisPost> Posts { get; set; }
    }
}
