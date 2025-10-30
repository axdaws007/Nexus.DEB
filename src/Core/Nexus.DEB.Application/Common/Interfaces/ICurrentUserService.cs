using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        Guid PostId { get; }
        bool IsAuthenticated { get; }

        Task<UserDetails?> GetUserDetailsAsync();
    }
}
