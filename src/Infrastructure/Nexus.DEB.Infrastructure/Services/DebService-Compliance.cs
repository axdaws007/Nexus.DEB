using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Compliance;
using Nexus.DEB.Domain.Models;
using Task = System.Threading.Tasks.Task;

namespace Nexus.DEB.Infrastructure.Services
{
    public partial class DebService
    {
        #region Compliance Tree - Configuration

        public async Task<IReadOnlyList<ComplianceState>> GetActiveComplianceStatesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.ComplianceStates
                .AsNoTracking()
                .Where(cs => cs.IsActive)
                .OrderBy(cs => cs.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ComplianceStateMapping>> GetComplianceStateMappingsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.ComplianceStateMappings
                .AsNoTracking()
                .Include(m => m.ComplianceState)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<BubbleUpRule>> GetActiveBubbleUpRulesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.BubbleUpRules
                .AsNoTracking()
                .Where(r => r.IsActive)
                .Include(r => r.ChildComplianceState)
                .Include(r => r.ResultComplianceState)
                .OrderBy(r => r.ParentNodeType)
                .ThenBy(r => r.Ordinal)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<NodeDefault>> GetNodeDefaultsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.NodeDefaults
                .AsNoTracking()
                .Include(d => d.DefaultComplianceState)
                .ToListAsync(cancellationToken);
        }

        #endregion

        //#region Compliance Tree - Structural Queries (for rebuild)

        public async Task<IReadOnlyList<Section>> GetSectionsByStandardVersionIdAsync(Guid standardVersionId, CancellationToken cancellationToken = default)
            => await _dbContext.Sections.AsNoTracking().Where(x => x.StandardVersionId == standardVersionId).ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<SectionRequirement>> GetSectionRequirementsByStandardVersionIdAsync(Guid standardVersionId, CancellationToken cancellationToken = default)
            => await _dbContext.SectionRequirements
                        .AsNoTracking()
                        .Include(x => x.Section)
                        .Where(x => x.Section.StandardVersionId == standardVersionId)
                        .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<TreeIdentifier>> GetTreeIdentifiersForStatementAsync(
            Guid statementId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.StatementsRequirementsScopes
                .AsNoTracking()
                .Where(srs => srs.StatementId == statementId)
                .SelectMany(srs => srs.Requirement.StandardVersions
                    .Select(sv => new TreeIdentifier(sv.EntityId, srs.ScopeId)))
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        #region Compliance Tree - Node Operations

        public async Task<IReadOnlyList<ComplianceTreeNode>> GetTreeAsync(TreeIdentifier tree, CancellationToken cancellationToken = default)
            => await _dbContext.ComplianceTreeNodes
                .AsNoTracking()
                .Include(n => n.ComplianceState)
                .Include(n => n.Summaries)
                    .ThenInclude(s => s.ComplianceState)
                .Where(n => n.StandardVersionID == tree.StandardVersionId
                          && n.ScopeID == tree.ScopeId)
                .OrderBy(n => n.NodeType)
                    .ThenBy(n => n.ParentEntityID)
                    .ThenBy(n => n.EntityID)
                .ToListAsync(cancellationToken);

        public async Task<ComplianceTreeNode?> GetComplianceTreeNodeAsync(
            TreeIdentifier tree, string nodeType, Guid entityId, Guid? parentEntityId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.ComplianceTreeNodes
                .Include(n => n.ComplianceState)
                .FirstOrDefaultAsync(n =>
                    n.StandardVersionID == tree.StandardVersionId &&
                    n.ScopeID == tree.ScopeId &&
                    n.NodeType == nodeType &&
                    n.EntityID == entityId &&
                    n.ParentEntityID == parentEntityId,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<ComplianceTreeNode>> GetComplianceTreeNodesByEntityAsync(
            TreeIdentifier tree, string nodeType, Guid entityId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.ComplianceTreeNodes
                .Include(n => n.ComplianceState)
                .Where(n =>
                    n.StandardVersionID == tree.StandardVersionId &&
                    n.ScopeID == tree.ScopeId &&
                    n.NodeType == nodeType &&
                    n.EntityID == entityId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ComplianceTreeNode>> GetComplianceTreeNodesByEntityAcrossTreesAsync(
            Guid entityId, string nodeType,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.ComplianceTreeNodes
                .Include(n => n.ComplianceState)
                .Where(n =>
                    n.EntityID == entityId &&
                    n.NodeType == nodeType)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ComplianceTreeNode>> GetComplianceTreeChildrenAsync(
            TreeIdentifier tree, Guid parentEntityId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.ComplianceTreeNodes
                .AsNoTracking()
                .Where(n =>
                    n.StandardVersionID == tree.StandardVersionId &&
                    n.ScopeID == tree.ScopeId &&
                    n.ParentEntityID == parentEntityId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ComplianceTreeNode>> GetDescendantRequirementsAsync(
            TreeIdentifier tree, Guid ancestorEntityId,
            CancellationToken cancellationToken = default)
        {
            // Load all nodes for this tree and walk in memory
            // (same pattern as IsSectionDescendantOfAsync)
            var allNodes = await _dbContext.ComplianceTreeNodes
                .AsNoTracking()
                .Where(n =>
                    n.StandardVersionID == tree.StandardVersionId &&
                    n.ScopeID == tree.ScopeId)
                .ToListAsync(cancellationToken);

            var childLookup = allNodes.ToLookup(n => n.ParentEntityID);
            var results = new List<ComplianceTreeNode>();

            CollectDescendantRequirements(childLookup, ancestorEntityId, results);

            return results;
        }

        private static void CollectDescendantRequirements(
            ILookup<Guid?, ComplianceTreeNode> childLookup,
            Guid parentEntityId,
            List<ComplianceTreeNode> results)
        {
            foreach (var child in childLookup[parentEntityId])
            {
                if (child.NodeType == ComplianceNodeTypes.Requirement)
                    results.Add(child);

                if (child.NodeType == ComplianceNodeTypes.Section)
                    CollectDescendantRequirements(childLookup, child.EntityID, results);
            }
        }

        #endregion

        #region Compliance Tree - Mutations

        public async Task UpsertComplianceTreeNodeAsync(
            ComplianceTreeNode node, CancellationToken cancellationToken = default)
        {
            if (node.ComplianceTreeNodeID == 0)
            {
                await _dbContext.ComplianceTreeNodes.AddAsync(node, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpsertComplianceTreeNodesAsync(
            IEnumerable<ComplianceTreeNode> nodes, CancellationToken cancellationToken = default)
        {
            var nodeList = nodes.ToList();

            var newNodes = nodeList.Where(n => n.ComplianceTreeNodeID == 0).ToList();
            var existingNodes = nodeList.Where(n => n.ComplianceTreeNodeID != 0).ToList();

            if (newNodes.Count > 0)
                await _dbContext.ComplianceTreeNodes.AddRangeAsync(newNodes, cancellationToken);

            // Existing nodes are already tracked and will be updated on SaveChanges
            // (they were loaded with tracking in the query methods)

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveComplianceTreeNodeAsync(
            TreeIdentifier tree, string nodeType, Guid entityId, Guid? parentEntityId,
            CancellationToken cancellationToken = default)
        {
            await _dbContext.ComplianceTreeNodes
                .Where(n =>
                    n.StandardVersionID == tree.StandardVersionId &&
                    n.ScopeID == tree.ScopeId &&
                    n.NodeType == nodeType &&
                    n.EntityID == entityId &&
                    n.ParentEntityID == parentEntityId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task RemoveComplianceTreeNodesByEntityAsync(
            TreeIdentifier tree, string nodeType, Guid entityId,
            CancellationToken cancellationToken = default)
        {
            await _dbContext.ComplianceTreeNodes
                .Where(n =>
                    n.StandardVersionID == tree.StandardVersionId &&
                    n.ScopeID == tree.ScopeId &&
                    n.NodeType == nodeType &&
                    n.EntityID == entityId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task RemoveComplianceTreeAsync(
            TreeIdentifier tree, CancellationToken cancellationToken = default)
        {
            var ids = _dbContext.ComplianceTreeNodes
                .Where(n =>
                    n.StandardVersionID == tree.StandardVersionId &&
                    n.ScopeID == tree.ScopeId)
                .Select(n => n.ComplianceTreeNodeID);

            await _dbContext.ComplianceTreeNodeSummaries.Where(x => ids.Contains(x.ComplianceTreeNodeID)).ExecuteDeleteAsync(cancellationToken);

            await _dbContext.ComplianceTreeNodes
                .Where(n =>
                    n.StandardVersionID == tree.StandardVersionId &&
                    n.ScopeID == tree.ScopeId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task RemoveComplianceTreesByScopeAsync(
            Guid scopeId, CancellationToken cancellationToken = default)
        {
            await _dbContext.ComplianceTreeNodes
                .Where(n => n.ScopeID == scopeId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        #endregion

        #region Compliance Tree - Summaries

        public async Task ReplaceComplianceTreeNodeSummariesAsync(
            long complianceTreeNodeId,
            IEnumerable<ComplianceTreeNodeSummary> summaries,
            CancellationToken cancellationToken = default)
        {
            // Delete existing summaries
            await _dbContext.ComplianceTreeNodeSummaries
                .Where(s => s.ComplianceTreeNodeID == complianceTreeNodeId)
                .ExecuteDeleteAsync(cancellationToken);

            // Insert new summaries
            var summaryList = summaries.ToList();

            if (summaryList.Count > 0)
            {
                foreach (var summary in summaryList)
                    summary.ComplianceTreeNodeID = complianceTreeNodeId;

                await _dbContext.ComplianceTreeNodeSummaries.AddRangeAsync(summaryList, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        #endregion

        #region Compliance Tree - Full Tree Queries

        public async Task<ComplianceTreeResult> GetFilteredTreeAsync(ComplianceTreeQuery query, CancellationToken cancellationToken = default)
        {
            var allNodes = await GetTreeAsync(query.Tree, cancellationToken);
            var complianceStates = await GetActiveComplianceStatesAsync();

            var workingSet = allNodes.ToList();

            // ── Step 1: Hide empty Sections ──
            if (query.HideEmptySections)
            {
                workingSet = RemoveEmptySections(workingSet);
            }

            var hasComplianceFilter = query.ComplianceStateFilter is { Count: > 0 };

            // ── Step 2: No compliance filter — return working set as direct matches ──
            if (!hasComplianceFilter)
            {
                return new ComplianceTreeResult
                {
                    Nodes = workingSet.Select(n => new ComplianceTreeNodeResult
                    {
                        Node = n,
                        IsDirectMatch = true
                    }).ToList(),
                    ComplianceStates = complianceStates,
                    IsFiltered = false,
                    EmptySectionsHidden = query.HideEmptySections
                };
            }

            // ── Step 3: Apply compliance state filter ──
            var filterSet = query.ComplianceStateFilter!.ToHashSet();

            var directMatchIds = new HashSet<long>(
                workingSet
                    .Where(n => n.ComplianceStateID.HasValue && filterSet.Contains(n.ComplianceStateID.Value))
                    .Select(n => n.ComplianceTreeNodeID));

            // ── Step 4: Walk up from matches to preserve ancestor paths ──
            var visibleIds = new HashSet<long>(directMatchIds);

            foreach (var matchId in directMatchIds)
            {
                var current = workingSet.First(n => n.ComplianceTreeNodeID == matchId);

                while (current.ParentEntityID != null && current.ParentNodeType != null)
                {
                    var parent = workingSet.FirstOrDefault(n =>
                        n.NodeType == current.ParentNodeType &&
                        n.EntityID == current.ParentEntityID.Value);

                    if (parent == null)
                        break;

                    if (!visibleIds.Add(parent.ComplianceTreeNodeID))
                        break; // Already visited — ancestors above are already included

                    current = parent;
                }
            }

            // ── Step 5: Build result ──
            var resultNodes = workingSet
                .Where(n => visibleIds.Contains(n.ComplianceTreeNodeID))
                .Select(n => new ComplianceTreeNodeResult
                {
                    Node = n,
                    IsDirectMatch = directMatchIds.Contains(n.ComplianceTreeNodeID)
                })
                .ToList();

            return new ComplianceTreeResult
            {
                Nodes = resultNodes,
                ComplianceStates = complianceStates,
                IsFiltered = true,
                EmptySectionsHidden = query.HideEmptySections
            };
        }

        public async Task<IReadOnlyList<ComplianceTreeNode>> GetComplianceTreeAsync(
            TreeIdentifier tree, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ComplianceTreeNodes
                .AsNoTracking()
                .Include(n => n.ComplianceState)
                .Include(n => n.Summaries)
                    .ThenInclude(s => s.ComplianceState)
                .Where(n =>
                    n.StandardVersionID == tree.StandardVersionId &&
                    n.ScopeID == tree.ScopeId)
                .OrderBy(n => n.NodeType)
                    .ThenBy(n => n.ParentEntityID)
                    .ThenBy(n => n.EntityID)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TreeIdentifier>> GetTreesContainingEntityAsync(
            Guid entityId, string nodeType,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.ComplianceTreeNodes
                .AsNoTracking()
                .Where(n =>
                    n.EntityID == entityId &&
                    n.NodeType == nodeType)
                .Select(n => new TreeIdentifier(n.StandardVersionID, n.ScopeID))
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        #endregion

        private static List<ComplianceTreeNode> RemoveEmptySections(List<ComplianceTreeNode> nodes)
        {
            var emptyIds = new HashSet<long>();
            var changed = true;

            // Iteratively remove until stable (handles cascading parent removal)
            while (changed)
            {
                changed = false;

                foreach (var node in nodes)
                {
                    if (emptyIds.Contains(node.ComplianceTreeNodeID))
                        continue;

                    if (node.NodeType != ComplianceNodeTypes.Section)
                        continue;

                    // A Section is empty if TotalRequirementCount is 0 or null
                    var isEmpty = !node.TotalRequirementCount.HasValue
                                  || node.TotalRequirementCount.Value == 0;

                    if (!isEmpty)
                        continue;

                    // Also check if the Section has any non-empty children remaining
                    // (another Section that is not yet marked empty)
                    var hasNonEmptyChild = nodes.Any(n =>
                        !emptyIds.Contains(n.ComplianceTreeNodeID) &&
                        n.ParentEntityID == node.EntityID &&
                        n.NodeType != ComplianceNodeTypes.Section); // non-Section child = not empty

                    if (hasNonEmptyChild)
                        continue;

                    var hasNonEmptySectionChild = nodes.Any(n =>
                        !emptyIds.Contains(n.ComplianceTreeNodeID) &&
                        n.ParentEntityID == node.EntityID &&
                        n.NodeType == ComplianceNodeTypes.Section);

                    if (hasNonEmptySectionChild)
                        continue;

                    emptyIds.Add(node.ComplianceTreeNodeID);
                    changed = true;
                }
            }

            return nodes.Where(n => !emptyIds.Contains(n.ComplianceTreeNodeID)).ToList();
        }
    }
}