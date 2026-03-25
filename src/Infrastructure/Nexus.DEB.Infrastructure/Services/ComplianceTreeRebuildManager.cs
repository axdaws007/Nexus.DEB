using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Compliance;
using Nexus.DEB.Domain.Models.Enums;

namespace Nexus.DEB.Infrastructure.Services.Compliance
{
    public class ComplianceTreeRebuildManager : IComplianceTreeRebuildManager
    {
        private readonly IDebService _debService;
        private readonly ILogger<ComplianceTreeRebuildManager> _logger;

        public ComplianceTreeRebuildManager(
            IDebService debService,
            ILogger<ComplianceTreeRebuildManager> logger)
        {
            _debService = debService;
            _logger = logger;
        }

        public async Task RequestTreeRebuildAsync(
            TreeIdentifier tree, CancellationToken ct = default)
        {
            await _debService.UpsertRebuildRequestAsync(tree, ct);

            _logger.LogInformation(
                "Compliance tree rebuild requested for SV={StandardVersionId} Scope={ScopeId}",
                tree.StandardVersionId, tree.ScopeId);
        }

        public async Task RequestAllTreeRebuildsForStandardVersionAsync(
            Guid standardVersionId, CancellationToken ct = default)
        {
            var scopeIds = await _debService
                .GetScopeIdsByStandardVersionAsync(standardVersionId, ct);

            foreach (var scopeId in scopeIds)
            {
                await _debService.UpsertRebuildRequestAsync(
                    new TreeIdentifier(standardVersionId, scopeId), ct);
            }

            _logger.LogInformation(
                "Compliance tree rebuilds requested for SV={StandardVersionId}, " +
                "{Count} scope(s) affected",
                standardVersionId, scopeIds.Count);
        }

        public async Task<IReadOnlyList<TreeIdentifier>> GetEligibleRebuildsAsync(
            TimeSpan debounceWindow, CancellationToken ct = default)
        {
            var threshold = DateTime.UtcNow - debounceWindow;
            return await _debService.GetEligibleRebuildRequestsAsync(threshold, ct);
        }

        public async Task<Guid?> TryClaimRequestAsync(
            TreeIdentifier tree, CancellationToken ct = default)
        {
            var buildId = Guid.NewGuid();
            var claimed = await _debService
                .TryClaimRebuildRequestAsync(tree, buildId, ct);

            if (claimed)
            {
                _logger.LogInformation(
                    "Claimed compliance tree rebuild for SV={StandardVersionId} " +
                    "Scope={ScopeId}, BuildId={BuildId}",
                    tree.StandardVersionId, tree.ScopeId, buildId);
                return buildId;
            }

            return null;
        }

        public async Task<bool> IsStillBuildingAsync(
            TreeIdentifier tree, CancellationToken ct = default)
        {
            var status = await _debService
                .GetRebuildRequestStatusAsync(tree, ct);
            return status == ComplianceTreeRebuildStatus.Building;
        }

        public async Task PromoteBuildAsync(
            TreeIdentifier tree, Guid newBuildId, CancellationToken ct = default)
        {
            await _debService.PromoteAndCleanupBuildAsync(tree, newBuildId, ct);

            _logger.LogInformation(
                "Promoted BuildId={NewBuildId} for SV={StandardVersionId} Scope={ScopeId}",
                newBuildId, tree.StandardVersionId, tree.ScopeId);
        }

        public async Task AbandonBuildAsync(
            TreeIdentifier tree, Guid buildId, CancellationToken ct = default)
        {
            var deleted = await _debService
                .DeleteNodesByBuildIdAsync(buildId, ct);

            await _debService.ResetRebuildRequestToPendingAsync(tree, ct);

            _logger.LogInformation(
                "Abandoned BuildId={BuildId} for SV={StandardVersionId} " +
                "Scope={ScopeId}, deleted {Count} orphaned nodes",
                buildId, tree.StandardVersionId, tree.ScopeId, deleted);
        }
    }
}
