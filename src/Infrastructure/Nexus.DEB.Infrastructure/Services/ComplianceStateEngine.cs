using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Compliance;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Services
{
    public class ComplianceStateEngine : IComplianceStateEngine
    {
        private readonly IDebService _debService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ComplianceStateEngine> _logger;

        private const string ComplianceStateMappingsCacheKey = "ComplianceEngine:ComplianceStateMappings";
        private const string BubbleUpRulesCacheKey = "ComplianceEngine:BubbleUpRules";
        private const string NodeDefaultsCacheKey = "ComplianceEngine:NodeDefaults";
        private const string ComplianceStatesCacheKey = "ComplianceEngine:ComplianceStates";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

        public ComplianceStateEngine(
            IDebService debService,
            IMemoryCache cache,
            ILogger<ComplianceStateEngine> logger)
        {
            _debService = debService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<int?> ResolveComplianceStateAsync(WorkflowInfo workflowInfo)
        {
            var mappings = await GetComplianceStateMappingsAsync();

            var mapping = mappings
                .FirstOrDefault(m => m.WorkflowID == workflowInfo.WorkflowId && m.ActivityID == workflowInfo.ActivityId && m.StatusID == workflowInfo.StatusId);

            if (mapping == null)
            {
                _logger.LogWarning(
                    "No compliance state mapping found for WorkflowID={workflowId}, ActivityID={activityId}, StatusID={statusId}",
                    workflowInfo.WorkflowId, workflowInfo.ActivityId, workflowInfo.StatusId);
                return null;
            }

            return mapping.ComplianceStateID;
        }

        public async Task<ComplianceStateResult> EvaluateBubbleUpAsync(
            string parentNodeType,
            IReadOnlyCollection<int?> childComplianceStateIds)
        {
            if (childComplianceStateIds.Count == 0)
                return await GetDefaultAsync(parentNodeType, NodeDefaultScenario.NoChildren);

            var validStateIds = childComplianceStateIds
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            var rules = await GetBubbleUpRulesAsync(parentNodeType);

            foreach (var rule in rules)
            {
                bool matches = rule.Quantifier switch
                {
                    BubbleUpQuantifier.Any => validStateIds.Contains(rule.ChildComplianceStateID),
                    BubbleUpQuantifier.All => validStateIds.Count > 0
                        && validStateIds.All(id => id == rule.ChildComplianceStateID),
                    _ => false
                };

                if (matches)
                {
                    _logger.LogDebug(
                        "Bubble-up rule matched: ParentNodeType={ParentNodeType}, Rule={RuleId}, " +
                        "Quantifier={Quantifier}, ChildState={ChildState} → ResultState={ResultState}",
                        parentNodeType, rule.BubbleUpRuleID, rule.Quantifier,
                        rule.ChildComplianceStateID, rule.ResultComplianceStateID);

                    return ComplianceStateResult.FromState(rule.ResultComplianceStateID);
                }
            }

            _logger.LogDebug(
                "No bubble-up rule matched for ParentNodeType={ParentNodeType}, ChildStates=[{ChildStates}]",
                parentNodeType, string.Join(", ", validStateIds));

            return await GetDefaultAsync(parentNodeType, NodeDefaultScenario.NoRuleMatch);
        }

        public async Task<IReadOnlyList<ComplianceTreeNodeSummary>> CalculateRequirementAggregatesAsync(
            TreeIdentifier tree, Guid parentEntityId, Guid buildId, CancellationToken cancellationToken = default)
        {
            var descendants = await _debService.GetDescendantRequirementsAsync(
                tree, parentEntityId, buildId, cancellationToken);

            return BuildAggregates(descendants, ComplianceNodeTypes.Requirement);
        }

        public async Task<IReadOnlyList<ComplianceTreeNodeSummary>> CalculateSectionAggregatesAsync(
            TreeIdentifier tree, Guid rootEntityId, Guid buildId, CancellationToken cancellationToken = default)
        {
            var children = await _debService.GetComplianceTreeChildrenAsync(tree, rootEntityId, buildId, cancellationToken);

            var sectionChildren = children
                .Where(c => c.NodeType == ComplianceNodeTypes.Section)
                .ToList();

            return BuildAggregates(sectionChildren, ComplianceNodeTypes.Section);
        }

        public async Task<IReadOnlyList<ComplianceState>> GetActiveComplianceStatesAsync()
        {
            return await GetComplianceStatesAsync();
        }

        // ── Private helpers ──

        private static IReadOnlyList<ComplianceTreeNodeSummary> BuildAggregates(
            IReadOnlyList<ComplianceTreeNode> nodes, string childNodeType)
        {
            return nodes
                .Where(n => n.ComplianceStateID.HasValue)
                .GroupBy(n => n.ComplianceStateID!.Value)
                .Select(g => new ComplianceTreeNodeSummary
                {
                    ChildNodeType = childNodeType,
                    ComplianceStateID = g.Key,
                    Count = g.Count()
                })
                .ToList();
        }

        private async Task<IReadOnlyList<ComplianceStateMapping>> GetComplianceStateMappingsAsync()
        {
            return await _cache.GetOrCreateAsync(ComplianceStateMappingsCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                entry.Size = 1;
                return await _debService.GetComplianceStateMappingsAsync();
            }) ?? Array.Empty<ComplianceStateMapping>();
        }

        private async Task<IReadOnlyList<BubbleUpRule>> GetBubbleUpRulesAsync(string parentNodeType)
        {
            var allRules = await _cache.GetOrCreateAsync(BubbleUpRulesCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                entry.Size = 1;
                return await _debService.GetActiveBubbleUpRulesAsync();
            }) ?? Array.Empty<BubbleUpRule>();

            return allRules
                .Where(r => r.ParentNodeType == parentNodeType)
                .OrderBy(r => r.Ordinal)
                .ToList();
        }

        private async Task<IReadOnlyList<ComplianceState>> GetComplianceStatesAsync()
        {
            return await _cache.GetOrCreateAsync(ComplianceStatesCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                entry.Size = 1;
                return await _debService.GetActiveComplianceStatesAsync();
            }) ?? Array.Empty<ComplianceState>();
        }

        private async Task<ComplianceStateResult> GetDefaultAsync(string nodeType, string scenario)
        {
            var defaults = await _cache.GetOrCreateAsync(NodeDefaultsCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                entry.Size = 1;
                return await _debService.GetNodeDefaultsAsync();
            }) ?? Array.Empty<NodeDefault>();

            var nodeDefault = defaults
                .FirstOrDefault(d => d.NodeType == nodeType && d.Scenario == scenario);

            if (nodeDefault == null)
                return ComplianceStateResult.Empty;

            if (nodeDefault.DefaultComplianceStateID.HasValue)
                return ComplianceStateResult.FromState(nodeDefault.DefaultComplianceStateID.Value);

            if (!string.IsNullOrEmpty(nodeDefault.DefaultLabel))
                return ComplianceStateResult.FromLabel(nodeDefault.DefaultLabel);

            return ComplianceStateResult.Empty;
        }
    }
}
