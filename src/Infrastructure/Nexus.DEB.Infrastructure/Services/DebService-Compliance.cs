using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Models.Compliance;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Enums;
using Nexus.DEB.Domain.Models.Other;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
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

        public async Task<IReadOnlyList<ComplianceTreeNode>> GetComplianceTreeNodesByEntityAsync(
            TreeIdentifier tree, string nodeType, Guid entityId, Guid buildId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.ComplianceTreeNodes
                .Include(n => n.ComplianceState)
                .Where(n =>
                    n.StandardVersionID == tree.StandardVersionId &&
                    n.ScopeID == tree.ScopeId &&
                    n.NodeType == nodeType &&
                    n.EntityID == entityId &&
                    n.BuildId == buildId)
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
                    n.NodeType == nodeType &&
                    _dbContext.ComplianceTreeBuilds.Any(b =>
                        b.StandardVersionID == n.StandardVersionID &&
                        b.ScopeID == n.ScopeID &&
                        b.LiveBuildId == n.BuildId))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ComplianceTreeNode>> GetComplianceTreeChildrenAsync(
            TreeIdentifier tree, Guid parentEntityId, Guid buildId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.ComplianceTreeNodes
                .AsNoTracking()
                .Where(n =>
                    n.StandardVersionID == tree.StandardVersionId &&
                    n.ScopeID == tree.ScopeId &&
                    n.ParentEntityID == parentEntityId &&
                    n.BuildId == buildId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ComplianceTreeNode>> GetComplianceTreeChildrenAsync(
            TreeIdentifier tree, long parentTreeNodeId, Guid buildId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.ComplianceTreeNodes
                .AsNoTracking()
                .Where(n =>
                    n.StandardVersionID == tree.StandardVersionId &&
                    n.ScopeID == tree.ScopeId &&
                    n.ParentComplianceTreeNodeID == parentTreeNodeId &&
                    n.BuildId == buildId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ComplianceTreeNode>> GetDescendantRequirementsAsync(
            TreeIdentifier tree, Guid ancestorEntityId, Guid buildId,
            CancellationToken cancellationToken = default)
        {
            // Load all nodes for this tree and walk in memory
            // (same pattern as IsSectionDescendantOfAsync)
            var allNodes = await _dbContext.ComplianceTreeNodes
                .AsNoTracking()
                .Where(n =>
                    n.StandardVersionID == tree.StandardVersionId &&
                    n.ScopeID == tree.ScopeId &&
                    n.BuildId == buildId)
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

        public async Task<IReadOnlyList<ComplianceTreeNode>> GetComplianceTreeAsync(
            TreeIdentifier tree, Guid buildId, CancellationToken cancellationToken = default)
        => await _dbContext.ComplianceTreeNodes
                .AsNoTracking()
                .Include(n => n.ComplianceState)
                .Include(n => n.Summaries)
                    .ThenInclude(s => s.ComplianceState)
                .Where(n =>
                    n.StandardVersionID == tree.StandardVersionId &&
                    n.ScopeID == tree.ScopeId && 
                    n.BuildId == buildId)
                .OrderBy(n => n.NodeType)
                    .ThenBy(n => n.ParentEntityID)
                    .ThenBy(n => n.EntityID)
                .ToListAsync(cancellationToken);

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

        #region Compliance Tree Rebuild Requests

        public async Task UpsertRebuildRequestAsync(
            TreeIdentifier tree, CancellationToken ct = default)
        {
            var request = await _dbContext.ComplianceTreeRebuildRequests
                .FindAsync([tree.StandardVersionId, tree.ScopeId], ct);

            if (request == null)
            {
                request = new ComplianceTreeRebuildRequest
                {
                    StandardVersionID = tree.StandardVersionId,
                    ScopeID = tree.ScopeId,
                    Status = ComplianceTreeRebuildStatus.Pending,
                    RequestedAt = DateTime.UtcNow
                };
                _dbContext.ComplianceTreeRebuildRequests.Add(request);
            }
            else
            {
                request.Status = ComplianceTreeRebuildStatus.Pending;
                request.RequestedAt = DateTime.UtcNow;
                request.BuildId = null;
                request.StartedAt = null;
            }

            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<TreeIdentifier>> GetEligibleRebuildRequestsAsync(
            DateTime threshold, CancellationToken ct = default)
        {
            return await _dbContext.ComplianceTreeRebuildRequests
                .AsNoTracking()
                .Where(r =>
                    r.Status == ComplianceTreeRebuildStatus.Pending &&
                    r.RequestedAt <= threshold)
                .Select(r => new TreeIdentifier(r.StandardVersionID, r.ScopeID))
                .ToListAsync(ct);
        }

        public async Task<bool> TryClaimRebuildRequestAsync(
            TreeIdentifier tree, Guid buildId, CancellationToken ct = default)
        {
            var request = await _dbContext.ComplianceTreeRebuildRequests
                .FindAsync([tree.StandardVersionId, tree.ScopeId], ct);

            if (request == null || request.Status != ComplianceTreeRebuildStatus.Pending)
                return false;

            request.Status = ComplianceTreeRebuildStatus.Building;
            request.BuildId = buildId;
            request.StartedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<ComplianceTreeRebuildStatus?> GetRebuildRequestStatusAsync(
            TreeIdentifier tree, CancellationToken ct = default)
        {
            return await _dbContext.ComplianceTreeRebuildRequests
                .AsNoTracking()
                .Where(r =>
                    r.StandardVersionID == tree.StandardVersionId &&
                    r.ScopeID == tree.ScopeId)
                .Select(r => (ComplianceTreeRebuildStatus?)r.Status)
                .FirstOrDefaultAsync(ct);
        }

        public async Task ResetRebuildRequestToPendingAsync(
            TreeIdentifier tree, CancellationToken ct = default)
        {
            var request = await _dbContext.ComplianceTreeRebuildRequests
                .FindAsync([tree.StandardVersionId, tree.ScopeId], ct);

            if (request != null && request.Status == ComplianceTreeRebuildStatus.Building)
            {
                request.Status = ComplianceTreeRebuildStatus.Pending;
                request.BuildId = null;
                request.StartedAt = null;
                await _dbContext.SaveChangesAsync(ct);
            }
        }

        public async Task<BuildInfo?> GetCurrentLiveBuildInformationAsync(TreeIdentifier tree, CancellationToken cancellationToken = default)
            => await _dbContext.ComplianceTreeBuilds
                .AsNoTracking()
                .Where(b =>
                    b.StandardVersionID == tree.StandardVersionId &&
                    b.ScopeID == tree.ScopeId)
                .Select(b => new BuildInfo{ LiveBuildId = b.LiveBuildId, PromotedAt = b.PromotedAt })
                .FirstOrDefaultAsync(cancellationToken);

        #endregion

        #region Compliance Tree Builds

        public async Task<Guid?> GetLiveBuildIdAsync(
            TreeIdentifier tree, CancellationToken ct = default)
        {
            return await _dbContext.ComplianceTreeBuilds
                .AsNoTracking()
                .Where(b =>
                    b.StandardVersionID == tree.StandardVersionId &&
                    b.ScopeID == tree.ScopeId)
                .Select(b => (Guid?)b.LiveBuildId)
                .FirstOrDefaultAsync(ct);
        }

        public async Task PromoteAndCleanupBuildAsync(TreeIdentifier tree, Guid newBuildId, CancellationToken ct = default)
        {
            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(ct);

            try
            {
                // 1. Update or insert the ComplianceTreeBuild row
                var build = await _dbContext.ComplianceTreeBuilds
                    .FindAsync([tree.StandardVersionId, tree.ScopeId], ct);

                Guid? oldBuildId = null;

                if (build == null)
                {
                    build = new ComplianceTreeBuild
                    {
                        StandardVersionID = tree.StandardVersionId,
                        ScopeID = tree.ScopeId,
                        LiveBuildId = newBuildId,
                        PromotedAt = DateTime.UtcNow
                    };
                    _dbContext.ComplianceTreeBuilds.Add(build);
                }
                else
                {
                    oldBuildId = build.LiveBuildId;
                    build.LiveBuildId = newBuildId;
                    build.PromotedAt = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync(ct);

                // 2. Delete old build's nodes (cascade handles summaries)
                if (oldBuildId.HasValue)
                {
                    await _dbContext.ComplianceTreeNodes
                        .Where(n => n.BuildId == oldBuildId.Value)
                        .ExecuteDeleteAsync(ct);
                }

                // 3. Mark request as complete
                var request = await _dbContext.ComplianceTreeRebuildRequests
                    .FindAsync([tree.StandardVersionId, tree.ScopeId], ct);

                if (request != null)
                {
                    request.Status = ComplianceTreeRebuildStatus.Complete;
                    await _dbContext.SaveChangesAsync(ct);
                }

                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<int> DeleteNodesByBuildIdAsync(Guid buildId, CancellationToken ct = default)
        {
            return await _dbContext.ComplianceTreeNodes
                .Where(n => n.BuildId == buildId)
                .ExecuteDeleteAsync(ct);
        }

        #endregion
    }
}