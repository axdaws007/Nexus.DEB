using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Interfaces;
using Nexus.DEB.Domain.Models.Events;
using Nexus.DEB.Domain.Models.Other;

namespace Nexus.DEB.Infrastructure.Events.Subscribers
{
    public class EntitySavedDashboardSubscriber : IDomainEventSubscriber<EntitySavedEvent>
    {
        private readonly ILogger<EntitySavedDashboardSubscriber> _logger;
        private readonly IDebService _debService;
        private readonly ICurrentUserService _currentUserService;

        public EntitySavedDashboardSubscriber(
            ILogger<EntitySavedDashboardSubscriber> logger,
            IDebService debService,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _debService = debService;
            _currentUserService = currentUserService;
        }

        public string Name => "EntitySavedDashboard";
        public int Order => 150;

        public async Task HandleAsync(EntitySavedEvent @event, CancellationToken cancellationToken = default)
        {
            DateTime? dueDate = null;

            if (@event.Entity != null && @event.Entity is EntityDetailBase)
            {
                var entity = @event.Entity as EntityDetailBase;

                if (@event.Entity is TaskDetail)
                {
                    dueDate = ((TaskDetail)@event.Entity).DueDate;
                }

                var pawsInfo = await _debService.GetCurrentWorkflowStatusForEntityAsync(entity.EntityId, cancellationToken);

                DashboardInfo? dashboardInfo = null;

                if (@event.IsNew)
                {
                    dashboardInfo = new DashboardInfo();
                    dashboardInfo.EntityId = entity.EntityId;
                }
                else
                {
                    dashboardInfo = await _debService.GetDashboardInfoAsync(entity.EntityId, cancellationToken);
                }

                dashboardInfo.DueDate = dueDate;
                dashboardInfo.IsOpen = pawsInfo.PseudoStateTitle == DebHelper.Paws.States.Open;
                dashboardInfo.IsWorkflowActive = pawsInfo.StatusId == DebHelper.Paws.Status.Pending;

                if (@event.IsNew)
                    dashboardInfo = await _debService.CreateDashBoardInfoAsync(dashboardInfo, cancellationToken);
                else
                    dashboardInfo = await _debService.UpdateDashBoardInfoAsync(dashboardInfo, cancellationToken);
            }
        }
    }
}
