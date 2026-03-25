using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Compliance;
using Quartz;

namespace Nexus.DEB.Api.Jobs
{
    [DisallowConcurrentExecution]
    public class ComplianceTreeRebuildJob : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ComplianceTreeRebuildJob> _logger;
        private readonly TimeSpan _debounceWindow;

        public ComplianceTreeRebuildJob(
            IServiceScopeFactory scopeFactory,
            ILogger<ComplianceTreeRebuildJob> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            var seconds = configuration.GetValue("ComplianceTree:DebounceSeconds", 30);
            _debounceWindow = TimeSpan.FromSeconds(seconds);
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogDebug("Compliance tree rebuild job started");

            IReadOnlyList<TreeIdentifier> eligible;

            // Use a short-lived scope just for fetching the eligible list
            await using (var queryScope = _scopeFactory.CreateAsyncScope())
            {
                var manager = queryScope.ServiceProvider
                    .GetRequiredService<IComplianceTreeRebuildManager>();

                eligible = await manager.GetEligibleRebuildsAsync(
                    _debounceWindow, context.CancellationToken);
            }

            if (eligible.Count == 0)
            {
                _logger.LogDebug("No eligible compliance tree rebuilds found");
                return;
            }

            _logger.LogInformation(
                "Found {Count} eligible compliance tree rebuild(s)", eligible.Count);

            foreach (var tree in eligible)
            {
                if (context.CancellationToken.IsCancellationRequested)
                    break;

                await ProcessTreeRebuildAsync(tree, context.CancellationToken);
            }
        }

        private async Task ProcessTreeRebuildAsync(
            TreeIdentifier tree, CancellationToken ct)
        {
            // Each tree rebuild gets its own scope so DbContext state
            // doesn't leak between rebuilds
            await using var scope = _scopeFactory.CreateAsyncScope();

            var manager = scope.ServiceProvider
                .GetRequiredService<IComplianceTreeRebuildManager>();
            var recalculator = scope.ServiceProvider
                .GetRequiredService<IComplianceTreeRecalculator>();

            Guid? buildId = null;

            try
            {
                buildId = await manager.TryClaimRequestAsync(tree, ct);

                if (!buildId.HasValue)
                {
                    _logger.LogDebug(
                        "Could not claim rebuild for SV={StandardVersionId} " +
                        "Scope={ScopeId}, skipping (likely claimed by another instance)",
                        tree.StandardVersionId, tree.ScopeId);
                    return;
                }

                _logger.LogInformation(
                    "Starting compliance tree rebuild for SV={StandardVersionId} " +
                    "Scope={ScopeId}, BuildId={BuildId}",
                    tree.StandardVersionId, tree.ScopeId, buildId.Value);

                // The recalculator will need the BuildId so nodes are
                // written against the correct build, and a checkpoint
                // callback to check whether to continue
                await recalculator.RebuildTreeAsync(
                    tree,
                    buildId.Value,
                    checkpointCallback: async () =>
                    {
                        var stillBuilding = await manager.IsStillBuildingAsync(tree, ct);

                        if (!stillBuilding)
                        {
                            _logger.LogInformation(
                                "Rebuild for SV={StandardVersionId} Scope={ScopeId} " +
                                "was superseded, abandoning BuildId={BuildId}",
                                tree.StandardVersionId, tree.ScopeId, buildId.Value);
                        }

                        return stillBuilding;
                    },
                    ct);

                // Final check before promoting — a change could have arrived
                // between the last checkpoint and completion
                if (!await manager.IsStillBuildingAsync(tree, ct))
                {
                    _logger.LogInformation(
                        "Rebuild superseded after completion for SV={StandardVersionId} " +
                        "Scope={ScopeId}, abandoning BuildId={BuildId}",
                        tree.StandardVersionId, tree.ScopeId, buildId.Value);

                    await manager.AbandonBuildAsync(tree, buildId.Value, ct);
                    return;
                }

                await manager.PromoteBuildAsync(tree, buildId.Value, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "Compliance tree rebuild cancelled for SV={StandardVersionId} " +
                    "Scope={ScopeId}, BuildId={BuildId}",
                    tree.StandardVersionId, tree.ScopeId, buildId);

                if (buildId.HasValue)
                    await manager.AbandonBuildAsync(tree, buildId.Value, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Compliance tree rebuild failed for SV={StandardVersionId} " +
                    "Scope={ScopeId}, BuildId={BuildId}",
                    tree.StandardVersionId, tree.ScopeId, buildId);

                if (buildId.HasValue)
                    await manager.AbandonBuildAsync(tree, buildId.Value, ct);
            }
        }
    }
}
