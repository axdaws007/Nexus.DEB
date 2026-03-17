using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Services
{
    public class ComplianceTreeService : IComplianceTreeService
    {
        private readonly IDebService _debService;
        private readonly IComplianceStateEngine _engine;
        private readonly ILogger<ComplianceTreeService> _logger;

        public ComplianceTreeService(
            IDebService debService,
            IComplianceStateEngine engine,
            ILogger<ComplianceTreeService> logger)
        {
            _debService = debService;
            _engine = engine;
            _logger = logger;
        }

        public async Task<ComplianceTreeResult> GetFilteredTreeAsync(
            ComplianceTreeQuery query,
            CancellationToken cancellationToken = default)
        {
            // 1. Load the full tree
            var allNodes = await _debService.GetComplianceTreeAsync(query.Tree, cancellationToken);
            var complianceStates = await _engine.GetActiveComplianceStatesAsync();

            // 2. Build working list (mutable copy of the node set)
            var workingNodes = allNodes.ToList();

            // 3. Hide empty sections if requested
            if (query.HideEmptySections)
            {
                workingNodes = RemoveEmptySections(workingNodes);
            }

            // 4. Order for tree rendering (depth-first traversal)
            workingNodes = OrderForTreeRendering(workingNodes);

            var hasStateFilter = query.ComplianceStateFilter is { Count: > 0 };

            // 5. No compliance state filter — return everything as direct matches
            if (!hasStateFilter)
            {
                var lookup = BuildNodeIdLookup(workingNodes);

                return new ComplianceTreeResult
                {
                    Nodes = workingNodes.Select(n => new ComplianceTreeNodeResult
                    {
                        Node = n,
                        IsDirectMatch = true,
                        ParentComplianceTreeNodeID = ResolveParentTreeNodeId(n, lookup)
                    }).ToList(),
                    ComplianceStates = complianceStates,
                    IsFiltered = query.HideEmptySections
                };
            }

            var filterSet = query.ComplianceStateFilter!.ToHashSet();

            // 6. Identify direct matches
            var directMatchIds = new HashSet<long>(
                workingNodes
                    .Where(n => n.ComplianceStateID.HasValue &&
                                filterSet.Contains(n.ComplianceStateID.Value))
                    .Select(n => n.ComplianceTreeNodeID));

            // 7. Build lookup for ancestor walking
            var nodeById = workingNodes.ToDictionary(n => n.ComplianceTreeNodeID);

            // 8. Walk up from each direct match, collecting ancestor node IDs
            var visibleIds = new HashSet<long>(directMatchIds);

            foreach (var matchId in directMatchIds)
            {
                WalkUpToRoot(nodeById[matchId], workingNodes, visibleIds);
            }

            // 9. Build result
            var nodeLookup = BuildNodeIdLookup(workingNodes);

            var resultNodes = workingNodes
                .Where(n => visibleIds.Contains(n.ComplianceTreeNodeID))
                .Select(n => new ComplianceTreeNodeResult
                {
                    Node = n,
                    IsDirectMatch = directMatchIds.Contains(n.ComplianceTreeNodeID),
                    ParentComplianceTreeNodeID = ResolveParentTreeNodeId(n, nodeLookup)
                })
                .ToList();

            return new ComplianceTreeResult
            {
                Nodes = resultNodes,
                ComplianceStates = complianceStates,
                IsFiltered = true
            };
        }

        /// <summary>
        /// Orders nodes in depth-first traversal order for tree rendering.
        /// Root first, then children ordered by node type priority and ordinal.
        /// </summary>
        private static List<ComplianceTreeNode> OrderForTreeRendering(List<ComplianceTreeNode> nodes)
        {
            if (nodes.Count == 0) return nodes;

            var childLookup = nodes.ToLookup(n => n.ParentEntityID);
            var ordered = new List<ComplianceTreeNode>(nodes.Count);

            // Find the root (ParentEntityID is null)
            var root = nodes.FirstOrDefault(n => n.ParentEntityID == null);
            if (root == null) return nodes;

            TraverseDepthFirst(root, childLookup, ordered);

            // Safety: include any orphaned nodes not reached by traversal
            // (shouldn't happen, but prevents data loss)
            var orderedIds = new HashSet<long>(ordered.Select(n => n.ComplianceTreeNodeID));
            foreach (var node in nodes)
            {
                if (!orderedIds.Contains(node.ComplianceTreeNodeID))
                    ordered.Add(node);
            }

            return ordered;
        }

        private static void TraverseDepthFirst(
            ComplianceTreeNode node,
            ILookup<Guid?, ComplianceTreeNode> childLookup,
            List<ComplianceTreeNode> ordered)
        {
            ordered.Add(node);

            var children = childLookup[node.EntityID]
                .OrderBy(c => c.NodeType switch
                {
                    ComplianceNodeTypes.Section => 0,
                    ComplianceNodeTypes.Requirement => 1,
                    ComplianceNodeTypes.Statement => 2,
                    _ => 3
                })
                .ThenBy(c => c.Ordinal)
                .ThenBy(c => c.EntityID);

            foreach (var child in children)
            {
                TraverseDepthFirst(child, childLookup, ordered);
            }
        }

        /// <summary>
        /// Removes Section nodes that have no descendant Requirement nodes.
        /// Cascades upward — a Section whose only children were empty Sections
        /// is also removed.
        /// </summary>
        private static List<ComplianceTreeNode> RemoveEmptySections(List<ComplianceTreeNode> nodes)
        {
            var childLookup = nodes.ToLookup(n => n.ParentEntityID);
            var emptyIds = new HashSet<long>();

            // Find sections with no descendant requirements
            var sections = nodes
                .Where(n => n.NodeType == ComplianceNodeTypes.Section)
                .ToList();

            bool changed;
            do
            {
                changed = false;
                foreach (var section in sections)
                {
                    if (emptyIds.Contains(section.ComplianceTreeNodeID))
                        continue;

                    var children = childLookup[section.EntityID]
                        .Where(c => !emptyIds.Contains(c.ComplianceTreeNodeID))
                        .ToList();

                    var hasContent = children.Any(c =>
                        c.NodeType == ComplianceNodeTypes.Requirement ||
                        (c.NodeType == ComplianceNodeTypes.Section &&
                         !emptyIds.Contains(c.ComplianceTreeNodeID)));

                    if (!hasContent)
                    {
                        emptyIds.Add(section.ComplianceTreeNodeID);
                        changed = true;
                    }
                }
            } while (changed);

            return nodes
                .Where(n => !emptyIds.Contains(n.ComplianceTreeNodeID))
                .ToList();
        }

        /// <summary>
        /// Walks from a node up through its ancestors to the root,
        /// adding each ancestor to the visible set.
        /// </summary>
        private static void WalkUpToRoot(
            ComplianceTreeNode node,
            List<ComplianceTreeNode> allNodes,
            HashSet<long> visibleIds)
        {
            var current = node;

            while (current.ParentEntityID != null && current.ParentNodeType != null)
            {
                var parent = allNodes.FirstOrDefault(n =>
                    n.NodeType == current.ParentNodeType &&
                    n.EntityID == current.ParentEntityID.Value);

                if (parent == null)
                    break;

                if (!visibleIds.Add(parent.ComplianceTreeNodeID))
                    break; // Already visited — ancestors above are already included

                current = parent;
            }
        }

        private static Dictionary<(string NodeType, Guid EntityID), long> BuildNodeIdLookup(
            List<ComplianceTreeNode> nodes)
        {
            var lookup = new Dictionary<(string, Guid), long>();

            foreach (var node in nodes)
            {
                var key = (node.NodeType, node.EntityID);
                lookup.TryAdd(key, node.ComplianceTreeNodeID);
            }

            return lookup;
        }

        private static long? ResolveParentTreeNodeId(
            ComplianceTreeNode node,
            Dictionary<(string NodeType, Guid EntityID), long> lookup)
        {
            if (node.ParentNodeType == null || node.ParentEntityID == null)
                return null;

            return lookup.TryGetValue((node.ParentNodeType, node.ParentEntityID.Value), out var parentId)
                ? parentId
                : null;
        }
    }
}