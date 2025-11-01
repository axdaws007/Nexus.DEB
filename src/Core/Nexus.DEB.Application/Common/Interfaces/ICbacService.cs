using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ICbacService
    {
        Task<List<CbacCapability>> GetCapabilitiesAsync(Guid moduleId);
    }
}
