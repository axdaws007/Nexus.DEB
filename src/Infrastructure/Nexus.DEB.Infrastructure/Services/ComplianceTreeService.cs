using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Compliance;
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

            // 2. Build working list
            var workingNodes = allNodes.ToList();

            // 3. Hide empty sections if requested
            if (query.HideEmptySections)
            {
                workingNodes = RemoveEmptySections(workingNodes);
            }

            // 4. Build traversal entries (node + resolved parent ID)
            var traversalEntries = BuildTraversalEntries(workingNodes);

            var hasStateFilter = query.ComplianceStateFilter is { Count: > 0 };

            // 5. No compliance state filter — return everything as direct matches
            if (!hasStateFilter)
            {
                return new ComplianceTreeResult
                {
                    Nodes = traversalEntries.Select(e => new ComplianceTreeNodeResult
                    {
                        Node = e.Node,
                        IsDirectMatch = true
                    }).ToList(),
                    ComplianceStates = complianceStates,
                    IsFiltered = query.HideEmptySections
                };
            }

            var filterSet = query.ComplianceStateFilter!.ToHashSet();

            // 6. Identify direct match entries (by index, since a node can appear multiple times)
            var directMatchIndexes = new HashSet<int>();
            for (var i = 0; i < traversalEntries.Count; i++)
            {
                var node = traversalEntries[i].Node;
                if (node.ComplianceStateID.HasValue && filterSet.Contains(node.ComplianceStateID.Value))
                {
                    directMatchIndexes.Add(i);
                }
            }

            // 7. Walk up from each direct match, collecting visible indexes
            var visibleIndexes = new HashSet<int>(directMatchIndexes);

            foreach (var matchIndex in directMatchIndexes)
            {
                WalkUpEntries(matchIndex, traversalEntries, visibleIndexes);
            }

            // 8. Build result (preserves depth-first order)
            var resultNodes = new List<ComplianceTreeNodeResult>();
            for (var i = 0; i < traversalEntries.Count; i++)
            {
                if (visibleIndexes.Contains(i))
                {
                    resultNodes.Add(new ComplianceTreeNodeResult
                    {
                        Node = traversalEntries[i].Node,
                        IsDirectMatch = directMatchIndexes.Contains(i)
                    });
                }
            }

            return new ComplianceTreeResult
            {
                Nodes = resultNodes,
                ComplianceStates = complianceStates,
                IsFiltered = true
            };
        }

        /// <summary>
        /// Walks up from an entry to the root, adding ancestor indexes to the visible set.
        /// Uses the traversal's parent chain which correctly handles multi-parent nodes.
        /// </summary>
        private static void WalkUpEntries(
            int entryIndex,
            List<TraversalEntry> entries,
            HashSet<int> visibleIndexes)
        {
            var current = entries[entryIndex];

            while (current.ParentComplianceTreeNodeID != null)
            {
                // Find the parent entry — it will be earlier in the list (depth-first)
                // and have the matching ComplianceTreeNodeID
                var parentIndex = -1;
                for (var i = entryIndex - 1; i >= 0; i--)
                {
                    if (entries[i].Node.ComplianceTreeNodeID == current.ParentComplianceTreeNodeID)
                    {
                        parentIndex = i;
                        break;
                    }
                }

                if (parentIndex < 0)
                    break;

                if (!visibleIndexes.Add(parentIndex))
                    break; // Already visited — ancestors above are already included

                current = entries[parentIndex];
                entryIndex = parentIndex;
            }
        }

        private static List<TraversalEntry> BuildTraversalEntries(List<ComplianceTreeNode> nodes)
        {
            if (nodes.Count == 0) return [];

            var childLookup = nodes.ToLookup(n => n.ParentComplianceTreeNodeID);
            var entries = new List<TraversalEntry>(nodes.Count);

            var root = nodes.FirstOrDefault(n => n.ParentComplianceTreeNodeID == null);
            if (root == null) return nodes.Select(n => new TraversalEntry(n, null)).ToList();

            TraverseDepthFirst(root, childLookup, entries);

            // Safety: include any orphaned nodes
            var includedIds = new HashSet<long>(entries.Select(e => e.Node.ComplianceTreeNodeID));
            foreach (var node in nodes)
            {
                if (!includedIds.Contains(node.ComplianceTreeNodeID))
                    entries.Add(new TraversalEntry(node, null));
            }

            return entries;
        }

        private static void TraverseDepthFirst(
            ComplianceTreeNode node,
            ILookup<long?, ComplianceTreeNode> childLookup,
            List<TraversalEntry> entries)
        {
            entries.Add(new TraversalEntry(node, node.ParentComplianceTreeNodeID));

            var children = childLookup[node.ComplianceTreeNodeID]
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
                TraverseDepthFirst(child, childLookup, entries);
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
    }
}