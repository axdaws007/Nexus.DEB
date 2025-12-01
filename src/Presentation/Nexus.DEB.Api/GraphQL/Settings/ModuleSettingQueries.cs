using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Api.GraphQL.Settings
{
    [QueryType]
    public static class ModuleSettingQueries
    {
        [Authorize]
        public static async Task<List<Guid>> GetDefaultOwnerRoleIdsForEntity(
            string entityType,
            IDebService debService,
            IApplicationSettingsService applicationSettingsService,
            CancellationToken cancellationToken = default)
        {
            var moduleId = applicationSettingsService.GetModuleId("DEB");

            return await debService.GetDefaultOwnerRoleIdsForEntityTypeAsync(moduleId, entityType, cancellationToken);
        }
    }
}
