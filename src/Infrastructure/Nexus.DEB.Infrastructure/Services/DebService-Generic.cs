using Microsoft.EntityFrameworkCore;

namespace Nexus.DEB.Infrastructure.Services
{
    public partial class DebService
    {
        public async Task<Guid?> GetWorkflowId(Guid moduleId, string entityType)
        {
            var settingName = $"PawsWorkFlowID:{entityType}";

            var value = await _dbContext.ModuleSettings
                .AsNoTracking()
                .Where(x => x.ModuleId == moduleId && x.Name == settingName)
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            return Guid.TryParse(value, out var result) ? result : (Guid?)null;
        }
    }
}
