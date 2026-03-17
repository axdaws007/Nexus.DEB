using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Events;
using Nexus.DEB.Domain.Interfaces;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using Task = System.Threading.Tasks.Task;

namespace Nexus.DEB.Infrastructure.Events.Subscribers;

public class WorkflowTransitionCompletedComplianceSubscriber
    : IDomainEventSubscriber<WorkflowTransitionCompletedEvent>
{
    private readonly ILogger<WorkflowTransitionCompletedComplianceSubscriber> _logger;
    private readonly IComplianceTreeRecalculator _recalculator;

    public string Name => "WorkflowTransitionCompletedCompliance";
    public int Order => 60;

    public WorkflowTransitionCompletedComplianceSubscriber(
        ILogger<WorkflowTransitionCompletedComplianceSubscriber> logger,
        IComplianceTreeRecalculator recalculator)
    {
        _logger = logger;
        _recalculator = recalculator;
    }

    public async Task HandleAsync(
        WorkflowTransitionCompletedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var nodeType = MapEntityTypeToNodeType(@event.EntityType);
        if (nodeType == null)
        {
            _logger.LogDebug(
                "Entity type {EntityType} not mapped to compliance node type, skipping",
                @event.EntityType);
            return;
        }

        _logger.LogInformation(
            "Processing workflow transition compliance update for {EntityType} {EntityId}",
            @event.EntityType, @event.EntityId);

        try
        {
            await _recalculator.RecalculateFromEntityAsync(
                @event.EntityId, @event.EntityType, nodeType, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to update compliance tree for {EntityType} {EntityId}",
                @event.EntityType, @event.EntityId);
        }
    }

    private static string? MapEntityTypeToNodeType(string entityType) => entityType switch
    {
        EntityTypes.SoC => ComplianceNodeTypes.Statement,
        EntityTypes.Requirement => ComplianceNodeTypes.Requirement,
        EntityTypes.StandardVersion => ComplianceNodeTypes.StandardVersion,
        _ => null
    };
}