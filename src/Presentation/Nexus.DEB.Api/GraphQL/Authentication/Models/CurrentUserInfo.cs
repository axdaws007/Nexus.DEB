using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL.Authentication.Models
{
    public class CurrentUserInfo
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PostTitle { get; set; } = string.Empty;
        public ICollection<CisPost>? Posts { get; set; }
        public ICollection<string> Capabilities { get; set; } = new List<string>();

        public bool IsAuthenticated { get; set; }
    }
}
