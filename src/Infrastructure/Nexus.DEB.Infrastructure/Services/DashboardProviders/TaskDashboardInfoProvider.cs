using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Infrastructure.Services
{
    public class TaskDashboardInfoProvider : DashboardInfoProviderBase
    {
        public TaskDashboardInfoProvider(IPawsService pawsService, IDebService debService, IApplicationSettingsService applicationSettingsService) 
            : base(pawsService, debService, applicationSettingsService, EntityTypes.Task)
        {
        }

        protected override DateOnly? DetermineDueDate(object entity, Guid entityId)
        {
            if (entity is TaskDetail taskDetail)
            {
                return taskDetail.DueDate;
            }
            else if (entity is Domain.Models.Task task)
            {
                return task.DueDate;
            }

            return null;
        }
    }
}
