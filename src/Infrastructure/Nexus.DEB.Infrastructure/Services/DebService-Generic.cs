using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Services
{
    public partial class DebService
    {
        public async Task<Guid?> GetWorkflowIdAsync(
            Guid moduleId, 
            string entityType, 
            CancellationToken cancellationToken = default)
        {
            var settingName = $"PawsWorkFlowID:{entityType}";

            var value = await _dbContext.ModuleSettings
                .AsNoTracking()
                .Where(x => x.ModuleId == moduleId && x.Name == settingName)
                .Select(x => x.Value)
                .FirstOrDefaultAsync(cancellationToken);

            return Guid.TryParse(value, out var result) ? result : (Guid?)null;
        }

        public async Task<PawsState?> GetWorkflowStatusByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
            => await _dbContext.PawsStates.FirstOrDefaultAsync(x => x.EntityId == id, cancellationToken);
    }
}
