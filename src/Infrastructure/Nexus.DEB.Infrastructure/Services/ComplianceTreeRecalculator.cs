using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Compliance;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using Nexus.DEB.Infrastructure.Helpers;
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

            var scopeRequirementIds = await _debService.GetRequirementIdsByScopeAsync(tree.ScopeId, cancellationToken);
            var inScopeRequirementIds = new HashSet<Guid>(scopeRequirementIds);

            var allSections = await _debService.GetSectionsByStandardVersionIdAsync(tree.StandardVersionId, cancellationToken);
            var sectionRequirements = await _debService.GetSectionRequirementsByStandardVersionIdAsync(tree.StandardVersionId, cancellationToken);
            var statementLinks = await _debService.GetStatementRequirementLinksByScopeAsync(tree.ScopeId, cancellationToken);

            // Batch load entity details for labels
            var requirementLookup = await _debService.GetEntityHeadsAsync(inScopeRequirementIds, cancellationToken);

            var statementIds = statementLinks
                .Where(link => inScopeRequirementIds.Contains(link.RequirementId))
                .Select(link => link.StatementId)
                .Distinct()
                .ToList();

            var statementLookup = await _debService.GetEntityHeadsAsync(statementIds, cancellationToken);

            // ── Level 0: Root ──
            var rootNode = new ComplianceTreeNode
            {
                StandardVersionID = tree.StandardVersionId,
                ScopeID = tree.ScopeId,
                NodeType = ComplianceNodeTypes.StandardVersion,
                EntityID = tree.StandardVersionId,
                ParentNodeType = null,
                ParentEntityID = null,
                ParentComplianceTreeNodeID = null,
                NodeLabel = StringHelper.Truncate(standardVersion.Title, 150),
                NodeReference = standardVersion.SerialNumber,
                Ordinal = 0,
                LastCalculatedAt = _dateTimeProvider.Now
            };

            await _debService.UpsertComplianceTreeNodeAsync(rootNode, cancellationToken);

            // ── Level 1+: Sections (insert by depth, shallowest first) ──
            var sectionsByDepth = OrderSectionsByDepthAscending(allSections);
            var sectionNodeLookup = new Dictionary<Guid, long>(); // Section.Id → ComplianceTreeNodeID

            foreach (var section in sectionsByDepth)
            {
                long parentTreeNodeId;
                if (section.ParentSectionId.HasValue)
                {
                    if (!sectionNodeLookup.TryGetValue(section.ParentSectionId.Value, out parentTreeNodeId))
                        continue; // Parent not found — skip
                }
                else
                {
                    parentTreeNodeId = rootNode.ComplianceTreeNodeID;
                }

                var sectionNode = new ComplianceTreeNode
                {
                    StandardVersionID = tree.StandardVersionId,
                    ScopeID = tree.ScopeId,
                    NodeType = ComplianceNodeTypes.Section,
                    EntityID = section.Id,
                    ParentNodeType = section.ParentSectionId.HasValue
                        ? ComplianceNodeTypes.Section
                        : ComplianceNodeTypes.StandardVersion,
                    ParentEntityID = section.ParentSectionId ?? tree.StandardVersionId,
                    ParentComplianceTreeNodeID = parentTreeNodeId,
                    NodeLabel = StringHelper.Truncate(section.Title, 150),
                    NodeReference = section.Reference,
                    Ordinal = section.Ordinal,
                    LastCalculatedAt = _dateTimeProvider.Now
                };

                await _debService.UpsertComplianceTreeNodeAsync(sectionNode, cancellationToken);
                sectionNodeLookup[section.Id] = sectionNode.ComplianceTreeNodeID;
            }

            // ── Requirement level: one per SectionRequirement link ──
            // A Requirement in two Sections = two tree nodes
            var treeRequirementIds = new HashSet<Guid>();
            var requirementNodeLookup = new Dictionary<(Guid SectionId, Guid RequirementId), long>();

            foreach (var sr in sectionRequirements.Where(sr => sr.IsEnabled))
            {
                if (!inScopeRequirementIds.Contains(sr.RequirementID))
                    continue;

                if (!sectionNodeLookup.TryGetValue(sr.SectionID, out var parentSectionTreeNodeId))
                    continue;

                requirementLookup.TryGetValue(sr.RequirementID, out var requirement);

                var reqNode = new ComplianceTreeNode
                {
                    StandardVersionID = tree.StandardVersionId,
                    ScopeID = tree.ScopeId,
                    NodeType = ComplianceNodeTypes.Requirement,
                    EntityID = sr.RequirementID,
                    ParentNodeType = ComplianceNodeTypes.Section,
                    ParentEntityID = sr.SectionID,
                    ParentComplianceTreeNodeID = parentSectionTreeNodeId,
                    NodeLabel = StringHelper.Truncate(requirement?.Title, 150),
                    NodeReference = requirement?.SerialNumber,
                    Ordinal = sr.Ordinal,
                    LastCalculatedAt = _dateTimeProvider.Now
                };

                await _debService.UpsertComplianceTreeNodeAsync(reqNode, cancellationToken);
                requirementNodeLookup[(sr.SectionID, sr.RequirementID)] = reqNode.ComplianceTreeNodeID;
                treeRequirementIds.Add(sr.RequirementID);
            }

            // ── Statement level: one per Requirement tree node instance ──
            var statementEntityIds = new HashSet<Guid>();

            foreach (var link in statementLinks)
            {
                if (!treeRequirementIds.Contains(link.RequirementId))
                    continue;

                statementLookup.TryGetValue(link.StatementId, out var statement);

                // Find ALL Requirement tree nodes for this RequirementId
                // (there may be multiple if the Requirement is in multiple Sections)
                var reqTreeNodes = requirementNodeLookup
                    .Where(kvp => kvp.Key.RequirementId == link.RequirementId)
                    .ToList();

                foreach (var reqTreeNode in reqTreeNodes)
                {
                    var stmtNode = new ComplianceTreeNode
                    {
                        StandardVersionID = tree.StandardVersionId,
                        ScopeID = tree.ScopeId,
                        NodeType = ComplianceNodeTypes.Statement,
                        EntityID = link.StatementId,
                        ParentNodeType = ComplianceNodeTypes.Requirement,
                        ParentEntityID = link.RequirementId,
                        ParentComplianceTreeNodeID = reqTreeNode.Value,
                        NodeLabel = StringHelper.Truncate(statement?.Title, 150),
                        NodeReference = statement?.SerialNumber,
                        Ordinal = 0,
                        LastCalculatedAt = _dateTimeProvider.Now
                    };

                    await _debService.UpsertComplianceTreeNodeAsync(stmtNode, cancellationToken);
                }

                statementEntityIds.Add(link.StatementId);
            }

            // ── Resolve intrinsic compliance states ──
            await ResolveIntrinsicStatesInBatch(
                tree, statementEntityIds, EntityTypes.SoC, ComplianceNodeTypes.Statement, cancellationToken);
            await ResolveIntrinsicStatesInBatch(
                tree, treeRequirementIds, EntityTypes.Requirement, ComplianceNodeTypes.Requirement, cancellationToken);
            await ResolveIntrinsicStatesInBatch(
                tree, new HashSet<Guid> { tree.StandardVersionId }, EntityTypes.StandardVersion,
                ComplianceNodeTypes.StandardVersion, cancellationToken);

            // ── Bubble up bottom-to-top ──
            await RecalculateAllParentsBottomUp(tree, allSections, sectionRequirements, treeRequirementIds, cancellationToken);

            // Count total nodes for logging
            var totalNodes = 1 + sectionNodeLookup.Count + requirementNodeLookup.Count
                + statementEntityIds.Count; // approximate — statements may have multiple nodes

            _logger.LogInformation(
                "Compliance tree rebuilt for SV={StandardVersionId} Scope={ScopeId}: ~{NodeCount} nodes",
                tree.StandardVersionId, tree.ScopeId, totalNodes);
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

                _logger.LogDebug(
                    "Processing Section {SectionId} (depth position {Index}), found {ChildCount} children with states [{States}]",
                    section.Id,
                    sectionsByDepth.IndexOf(section),
                    children.Count,
                    string.Join(", ", children.Select(c => $"{c.NodeType}:{c.ComplianceStateID}")));

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

        // Used by RebuildTreeAsync for level-by-level INSERT (parents before children)
        private static IList<Section> OrderSectionsByDepthAscending(IReadOnlyList<Section> sections)
        {
            var depthMap = BuildDepthMap(sections);
            return sections.OrderBy(s => depthMap[s.Id]).ThenBy(s => s.Ordinal).ToList();
        }

        // Used by RecalculateAllParentsBottomUp for BUBBLE-UP (children before parents)
        private static IList<Section> OrderSectionsByDepthDescending(IReadOnlyList<Section> sections)
        {
            var depthMap = BuildDepthMap(sections);
            return sections.OrderByDescending(s => depthMap[s.Id]).ThenBy(s => s.Ordinal).ToList();
        }

        // Shared depth calculation
        private static Dictionary<Guid, int> BuildDepthMap(IReadOnlyList<Section> sections)
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
            return depthMap;
        }
    }
}
