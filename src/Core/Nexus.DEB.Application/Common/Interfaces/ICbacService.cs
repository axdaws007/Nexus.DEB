using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ICbacService
    {
        Task<List<CbacCapability>> GetCapabilitiesAsync(Guid moduleId);
        Task<ICollection<Guid>?> GetRolePostIdsAsync(ICollection<Guid> roleIds);
        Task<ICollection<CbacRole>?> GetRolesForPostAsync(Guid postId);
    }
}
