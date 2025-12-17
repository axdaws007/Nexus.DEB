using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IApplicationSettingsService
    {
        Guid GetModuleId(string moduleName);
        Guid GetInstanceId();
        Guid GetLibraryId(string libraryName);
        AuditConfiguration GetAuditConfiguration();
    }
}
