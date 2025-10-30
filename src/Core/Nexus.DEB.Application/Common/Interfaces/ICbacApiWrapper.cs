using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ICbacApiWrapper
    {
        Task<List<CbacCapability>> GetCapabilitiesAsync(Guid moduleId, string authCookie);
    }
}
