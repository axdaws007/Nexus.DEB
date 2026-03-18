using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Compliance;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using Nexus.DEB.Infrastructure.Helpers;
using System.Data.SqlTypes;
using Task = System.Threading.Tasks.Task;

namespace Nexus.DEB.Infrastructure.Services
{
    public class ComplianceTreeRecalculator : IComplianceTreeRecalculator
    {
        private readonly IComplianceStateEngine _engine;
        private readonly IDebService _debService;
        private readonly IApplicationSettingsService _appSettings;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<ComplianceTreeRecalculator> _logger;

        public ComplianceTreeRecalculator(
            IComplianceStateEngine engine,
            IDebService debService,
            IApplicationSettingsService appSettings,
            IDateTimeProvider dateTimeProvider,
            ILogger<ComplianceTreeRecalculator> logger)
        {
            _engine = engine;
            _debService = debService;
            _appSettings = appSettings;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task RecalculateFromEntityAsync(
            Guid entityId, string entityType, string nodeType,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Recalculating compliance from entity {EntityType} {EntityId}",
                entityType, entityId);

            // 1. Get pseudostate and resolve compliance state
            var workflowInfo = await GetCurrentPawsActivityAndStatusAsync(entityId, entityType, cancellationToken);

            int? complianceStateId = workflowInfo != null
                ? await _engine.ResolveComplianceStateAsync(workflowInfo)
                : null;

            // 2. Find all tree nodes for this entity across ALL trees (SV + Scope combos)
            var nodes = await _debService.GetComplianceTreeNodesByEntityAcrossTreesAsync(entityId, nodeType, cancellationToken);

            if (nodes.Count == 0)
            {
                _logger.LogDebug("No tree nodes found for {EntityType} {EntityId}, skipping", entityType, entityId);
                return;
            }

            // 3. Update each node's compliance state
            foreach (var node in nodes)
            {
                node.ComplianceStateID = complianceStateId;
                node.ComplianceStateLabel = null;
                node.PseudoStateID = workflowInfo?.PseudoStateId;
                node.PseudoStateTitle = workflowInfo?.PseudoStateTitle;
                node.ActivityId = workflowInfo?.ActivityId;
                node.StatusId = workflowInfo?.StatusId;
                node.LastCalculatedAt = _dateTimeProvider.Now;
            }
            await _debService.UpsertComplianceTreeNodesAsync(nodes, cancellationToken);

            // 4. Bubble up each unique parent branch in each tree
            var parentBranches = nodes
                .Where(n => n.ParentEntityID.HasValue && n.ParentNodeType != null)
                .Select(n => new
                {
                    Tree = new TreeIdentifier(n.StandardVersionID, n.ScopeID),
                    n.ParentEntityID,
                    n.ParentNodeType
                })
                .Distinct()
                .ToList();

            foreach (var branch in parentBranches)
            {
                await BubbleUpAsync(
                    branch.Tree,
                    branch.ParentEntityID!.Value,
                    branch.ParentNodeType!,
                    cancellationToken);
            }

        }

        public async Task RecalculateFromParentAsync(
            TreeIdentifier tree, Guid parentEntityId, string parentNodeType,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Recalculating compliance from parent {ParentNodeType} {ParentEntityId} " +
                "in tree SV={StandardVersionId} Scope={ScopeId}",
                parentNodeType, parentEntityId, tree.StandardVersionId, tree.ScopeId);

            await BubbleUpAsync(tree, parentEntityId, parentNodeType, cancellationToken);
        }

        public async Task RebuildTreeAsync(TreeIdentifier tree, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Rebuilding compliance tree for SV={StandardVersionId} Scope={ScopeId}",
                tree.StandardVersionId, tree.ScopeId);

            // 1. Clear existing tree
            await _debService.RemoveComplianceTreeAsync(tree, cancellationToken);

            // 2. Load structural data
            var standardVersion = await _debService.GetStandardVersionByIdAsync(tree.StandardVersionId, cancellationToken);
            if (standardVersion == null)
            {
                _logger.LogWarning("StandardVersion {Id} not found, cannot rebuild", tree.StandardVersionId);
                return;
            }

            // 3. Get in-scope requirement IDs for this Scope
            var scopeRequirementIds = await _debService.GetRequirementIdsByScopeAsync(
                tree.ScopeId, cancellationToken);
            var inScopeRequirementIds = new HashSet<Guid>(scopeRequirementIds);

            // 4. Load sections and section-requirement links
            var allSections = await _debService.GetSectionsByStandardVersionIdAsync(
                tree.StandardVersionId, cancellationToken);
            var sectionRequirements = await _debService.GetSectionRequirementsByStandardVersionIdAsync(
                tree.StandardVersionId, cancellationToken);

            // 5. Load statement links scoped to this Scope
            var statementLinks = await _debService.GetStatementRequirementLinksByScopeAsync(
                tree.ScopeId, cancellationToken);

            var nodes = new List<ComplianceTreeNode>();

            // 6. Root node
            nodes.Add(new ComplianceTreeNode
            {
                StandardVersionID = tree.StandardVersionId,
                ScopeID = tree.ScopeId,
                NodeType = ComplianceNodeTypes.StandardVersion,
                EntityID = tree.StandardVersionId,
                ParentNodeType = null,
                ParentEntityID = null,
                NodeLabel = StringHelper.Truncate(standardVersion.Title, 150),
                NodeReference = standardVersion.SerialNumber ?? string.Empty,
                Ordinal = 0,
                LastCalculatedAt = _dateTimeProvider.Now
            });

            // 7. Section nodes (full hierarchy regardless of scope)
            foreach (var section in allSections)
            {
                nodes.Add(new ComplianceTreeNode
                {
                    StandardVersionID = tree.StandardVersionId,
                    ScopeID = tree.ScopeId,
                    NodeType = ComplianceNodeTypes.Section,
                    EntityID = section.Id,
                    ParentNodeType = section.ParentSectionId.HasValue
                        ? ComplianceNodeTypes.Section
                        : ComplianceNodeTypes.StandardVersion,
                    ParentEntityID = section.ParentSectionId ?? tree.StandardVersionId,
                    NodeLabel = StringHelper.Truncate(section.Title, 150),
                    NodeReference = section.Reference,
                    Ordinal = section.Ordinal,
                    LastCalculatedAt = _dateTimeProvider.Now
                });
            }

            // 8. Requirement nodes — only in-scope Requirements
            var requirementEntityHeads = await _debService.GetEntityHeadsAsync(inScopeRequirementIds, cancellationToken);

            var treeRequirementIds = new HashSet<Guid>();
            foreach (var sr in sectionRequirements.Where(sr => sr.IsEnabled))
            {
                if (!inScopeRequirementIds.Contains(sr.RequirementID))
                    continue;

                requirementEntityHeads.TryGetValue(sr.RequirementID, out var requirement);

                nodes.Add(new ComplianceTreeNode
                {
                    StandardVersionID = tree.StandardVersionId,
                    ScopeID = tree.ScopeId,
                    NodeType = ComplianceNodeTypes.Requirement,
                    EntityID = sr.RequirementID,
                    ParentNodeType = ComplianceNodeTypes.Section,
                    ParentEntityID = sr.SectionID,
                    NodeLabel = StringHelper.Truncate(requirement?.Title, 150),
                    NodeReference = requirement?.SerialNumber,
                    Ordinal = sr.Ordinal,
                    LastCalculatedAt = _dateTimeProvider.Now
                });
                treeRequirementIds.Add(sr.RequirementID);
            }

            // 9. Statement nodes — only via the specific Scope's StatementRequirementScope links
            var statementIds = statementLinks
                .Where(link => treeRequirementIds.Contains(link.RequirementId))
                .Select(link => link.StatementId)
                .Distinct()
                .ToList();

            var statementOrdinal = 0;
            var statementEntityHeads = await _debService.GetEntityHeadsAsync(statementIds, cancellationToken);
            var statementEntityIds = new HashSet<Guid>();
            foreach (var link in statementLinks)
            {
                if (!treeRequirementIds.Contains(link.RequirementId))
                    continue;

                statementEntityHeads.TryGetValue(link.StatementId, out var statement);

                nodes.Add(new ComplianceTreeNode
                {
                    StandardVersionID = tree.StandardVersionId,
                    ScopeID = tree.ScopeId,
                    NodeType = ComplianceNodeTypes.Statement,
                    EntityID = link.StatementId,
                    ParentNodeType = ComplianceNodeTypes.Requirement,
                    ParentEntityID = link.RequirementId,
                    NodeLabel = StringHelper.Truncate(statement?.Title, 150),
                    NodeReference = statement?.SerialNumber,
                    Ordinal = statementOrdinal++,
                    LastCalculatedAt = _dateTimeProvider.Now
                });
                statementEntityIds.Add(link.StatementId);
            }

            // 10. Batch insert all structural nodes
            await _debService.UpsertComplianceTreeNodesAsync(nodes, cancellationToken);

            // 11. Resolve intrinsic compliance states
            await ResolveIntrinsicStatesInBatch(
                tree, statementEntityIds, EntityTypes.SoC, ComplianceNodeTypes.Statement, cancellationToken);
            await ResolveIntrinsicStatesInBatch(
                tree, treeRequirementIds, EntityTypes.Requirement, ComplianceNodeTypes.Requirement, cancellationToken);
            await ResolveIntrinsicStatesInBatch(
                tree, new HashSet<Guid> { tree.StandardVersionId }, EntityTypes.StandardVersion,
                ComplianceNodeTypes.StandardVersion, cancellationToken);

            // 12. Bubble up bottom-to-top
            await RecalculateAllParentsBottomUp(tree, allSections, sectionRequirements, treeRequirementIds, cancellationToken);

            _logger.LogInformation(
                "Compliance tree rebuilt for SV={StandardVersionId} Scope={ScopeId}: {NodeCount} nodes",
                tree.StandardVersionId, tree.ScopeId, nodes.Count);
        }

        public async Task RebuildAllTreesForStandardVersionAsync(
            Guid standardVersionId, CancellationToken cancellationToken = default)
        {
            var scopeIds = await _debService.GetScopeIdsByStandardVersionAsync(
                standardVersionId, cancellationToken);

            _logger.LogInformation(
                "Rebuilding {Count} compliance trees for StandardVersion {Id}",
                scopeIds.Count, standardVersionId);

            foreach (var scopeId in scopeIds)
            {
                await RebuildTreeAsync(
                    new TreeIdentifier(standardVersionId, scopeId), cancellationToken);
            }
        }

        // ── Private helpers ──

        private async Task BubbleUpAsync(
            TreeIdentifier tree, Guid entityId, string nodeType,
            CancellationToken cancellationToken)
        {
            var currentEntityId = entityId;
            var currentNodeType = nodeType;
            const int maxDepth = 10;
            var depth = 0;

            while (depth < maxDepth)
            {
                depth++;

                // Get direct children for compliance state calculation
                var children = await _debService.GetComplianceTreeChildrenAsync(
                    tree, currentEntityId, cancellationToken);

                var childStateIds = children.Select(c => c.ComplianceStateID).ToList();

                // Evaluate bubble-up rules
                var result = await _engine.EvaluateBubbleUpAsync(currentNodeType, childStateIds);

                // Update the current node(s)
                var currentNodes = await _debService.GetComplianceTreeNodesByEntityAsync(
                    tree, currentNodeType, currentEntityId, cancellationToken);

                foreach (var node in currentNodes)
                {
                    node.ComplianceStateID = result.ComplianceStateID;
                    node.ComplianceStateLabel = result.Label;
                    node.LastCalculatedAt = _dateTimeProvider.Now;

                    // Calculate aggregates for Sections and StandardVersion
                    await RecalculateAggregatesAsync(node, tree, cancellationToken);
                }

                if (currentNodes.Count > 0)
                    await _debService.UpsertComplianceTreeNodesAsync(currentNodes, cancellationToken);

                // Move up to parent
                var firstNode = currentNodes.FirstOrDefault();
                if (firstNode?.ParentEntityID == null || firstNode.ParentNodeType == null)
                    break;

                currentEntityId = firstNode.ParentEntityID.Value;
                currentNodeType = firstNode.ParentNodeType;
            }
        }

        private async Task RecalculateAggregatesAsync(
            ComplianceTreeNode node, TreeIdentifier tree,
            CancellationToken cancellationToken)
        {
            var summaries = new List<ComplianceTreeNodeSummary>();

            switch (node.NodeType)
            {
                case ComplianceNodeTypes.Section:
                    var reqAggregates = await _engine.CalculateRequirementAggregatesAsync(
                        tree, node.EntityID, cancellationToken);
                    summaries.AddRange(reqAggregates);
                    node.TotalRequirementCount = reqAggregates.Sum(a => a.Count);

                    var sectionChildAggregates = await _engine.CalculateSectionAggregatesAsync(
                        tree, node.EntityID, cancellationToken);
                    summaries.AddRange(sectionChildAggregates);
                    node.TotalSectionCount = sectionChildAggregates.Sum(a => a.Count);
                    break;

                case ComplianceNodeTypes.StandardVersion:
                    var rootReqAggregates = await _engine.CalculateRequirementAggregatesAsync(
                        tree, node.EntityID, cancellationToken);
                    summaries.AddRange(rootReqAggregates);
                    node.TotalRequirementCount = rootReqAggregates.Sum(a => a.Count);

                    var sectionAggregates = await _engine.CalculateSectionAggregatesAsync(
                        tree, node.EntityID, cancellationToken);
                    summaries.AddRange(sectionAggregates);
                    node.TotalSectionCount = sectionAggregates.Sum(a => a.Count);
                    break;

                default:
                    return;
            }

            foreach (var s in summaries)
                s.ComplianceTreeNodeID = node.ComplianceTreeNodeID;

            await _debService.ReplaceComplianceTreeNodeSummariesAsync(
                node.ComplianceTreeNodeID, summaries, cancellationToken);
        }

        private async Task<WorkflowInfo?> GetCurrentPawsActivityAndStatusAsync(
            Guid entityId, string entityType, CancellationToken cancellationToken)
        {
            var moduleId = _appSettings.GetModuleId("DEB");
            var workflowId = await _debService.GetWorkflowIdAsync(moduleId, entityType, cancellationToken);
            if (!workflowId.HasValue) return null;

            // get entity's current paws info
            var detail = await _debService.GetCurrentWorkflowStatusForEntityAsync(entityId, cancellationToken);

            if (detail == null) return null;

            return new WorkflowInfo(workflowId.Value, detail.ActivityId, detail.StatusId, detail.PseudoStateId, detail.PseudoStateTitle);
        }

        private async Task ResolveIntrinsicStatesInBatch(
            TreeIdentifier tree,
            IReadOnlyCollection<Guid> entityIds,
            string entityType, string nodeType,
            CancellationToken cancellationToken)
        {
            if (entityIds.Count == 0) return;

            // TODO: Batch PAWS call would be more efficient
            foreach (var entityId in entityIds)
            {
                var workflowInfo = await GetCurrentPawsActivityAndStatusAsync(entityId, entityType, cancellationToken);
                int? complianceStateId = workflowInfo != null
                    ? await _engine.ResolveComplianceStateAsync(workflowInfo)
                    : null;

                var nodes = await _debService.GetComplianceTreeNodesByEntityAsync(
                    tree, nodeType, entityId, cancellationToken);

                foreach (var node in nodes)
                {
                    node.ComplianceStateID = complianceStateId;
                    node.PseudoStateID = workflowInfo?.PseudoStateId;
                    node.PseudoStateTitle = workflowInfo?.PseudoStateTitle;
                    node.ActivityId = workflowInfo?.ActivityId;
                    node.StatusId = workflowInfo?.StatusId;
                    node.LastCalculatedAt = _dateTimeProvider.Now;
                }

                if (nodes.Count > 0)
                    await _debService.UpsertComplianceTreeNodesAsync(nodes, cancellationToken);
            }
        }

        private async Task RecalculateAllParentsBottomUp(
            TreeIdentifier tree,
            IReadOnlyList<Section> allSections,
            IReadOnlyList<SectionRequirement> sectionRequirements,
            IReadOnlyCollection<Guid> inScopeRequirementIds,
            CancellationToken cancellationToken)
        {
            // 1. Requirements: recalculate from statement children
            var requirementIds = sectionRequirements
                .Where(sr => sr.IsEnabled && inScopeRequirementIds.Contains(sr.RequirementID))
                .Select(sr => sr.RequirementID)
                .Distinct();

            foreach (var reqId in requirementIds)
            {
                var children = await _debService.GetComplianceTreeChildrenAsync(tree, reqId, cancellationToken);
                var result = await _engine.EvaluateBubbleUpAsync(
                    ComplianceNodeTypes.Requirement, children.Select(c => c.ComplianceStateID).ToList());

                var reqNodes = await _debService.GetComplianceTreeNodesByEntityAsync(
                    tree, ComplianceNodeTypes.Requirement, reqId, cancellationToken);

                foreach (var n in reqNodes)
                {
                    n.ComplianceStateID = result.ComplianceStateID;
                    n.ComplianceStateLabel = result.Label;
                    n.LastCalculatedAt = _dateTimeProvider.Now;
                }
                if (reqNodes.Count > 0)
                    await _debService.UpsertComplianceTreeNodesAsync(reqNodes, cancellationToken);
            }

            // 2. Sections: process deepest first
            var sectionsByDepth = OrderSectionsByDepthDescending(allSections);

            foreach (var section in sectionsByDepth)
            {
                var children = await _debService.GetComplianceTreeChildrenAsync(tree, section.Id, cancellationToken);
                var result = await _engine.EvaluateBubbleUpAsync(
                    ComplianceNodeTypes.Section, children.Select(c => c.ComplianceStateID).ToList());

                var sectionNodes = await _debService.GetComplianceTreeNodesByEntityAsync(
                    tree, ComplianceNodeTypes.Section, section.Id, cancellationToken);

                foreach (var n in sectionNodes)
                {
                    n.ComplianceStateID = result.ComplianceStateID;
                    n.ComplianceStateLabel = result.Label;
                    n.LastCalculatedAt = _dateTimeProvider.Now;
                    await RecalculateAggregatesAsync(n, tree, cancellationToken);
                }
                if (sectionNodes.Count > 0)
                    await _debService.UpsertComplianceTreeNodesAsync(sectionNodes, cancellationToken);
            }

            // 3. Root
            var rootChildren = await _debService.GetComplianceTreeChildrenAsync(
                tree, tree.StandardVersionId, cancellationToken);
            var rootResult = await _engine.EvaluateBubbleUpAsync(
                ComplianceNodeTypes.StandardVersion,
                rootChildren.Select(c => c.ComplianceStateID).ToList());

            var rootNodes = await _debService.GetComplianceTreeNodesByEntityAsync(
                tree, ComplianceNodeTypes.StandardVersion, tree.StandardVersionId, cancellationToken);

            foreach (var n in rootNodes)
            {
                n.ComplianceStateID = rootResult.ComplianceStateID;
                n.ComplianceStateLabel = rootResult.Label;
                n.LastCalculatedAt = _dateTimeProvider.Now;
                await RecalculateAggregatesAsync(n, tree, cancellationToken);
            }
            if (rootNodes.Count > 0)
                await _debService.UpsertComplianceTreeNodesAsync(rootNodes, cancellationToken);
        }

        private static IReadOnlyList<Section> OrderSectionsByDepthDescending(IReadOnlyList<Section> sections)
        {
            var depthMap = new Dictionary<Guid, int>();

            int GetDepth(Section s)
            {
                if (depthMap.TryGetValue(s.Id, out var cached)) return cached;
                if (!s.ParentSectionId.HasValue) { depthMap[s.Id] = 0; return 0; }
                var parent = sections.FirstOrDefault(x => x.Id == s.ParentSectionId.Value);
                var depth = parent != null ? GetDepth(parent) + 1 : 0;
                depthMap[s.Id] = depth;
                return depth;
            }

            foreach (var s in sections) GetDepth(s);
            return sections.OrderByDescending(s => depthMap[s.Id]).ToList();
        }
    }
}
