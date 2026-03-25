using Nexus.DEB.Application.Common.Models.Compliance;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IComplianceTreeRebuildManager
    {
        /// <summary>
        /// Requests a rebuild for a single tree. Inserts a new Pending request
        /// or updates RequestedAt (and resets Status to Pending) if one already exists.
        /// Called by event subscribers.
        /// </summary>
        Task RequestTreeRebuildAsync(TreeIdentifier tree, CancellationToken ct = default);

        /// <summary>
        /// Requests rebuilds for all trees under a Standard Version.
        /// Resolves all Scope IDs for the Standard Version and calls
        /// RequestTreeRebuildAsync for each.
        /// Called by subscribers handling structural changes (section moves, etc.).
        /// </summary>
        Task RequestAllTreeRebuildsForStandardVersionAsync(
            Guid standardVersionId, CancellationToken ct = default);

        /// <summary>
        /// Finds all Pending requests where RequestedAt is older than the debounce
        /// threshold. Called by the Quartz job on each poll.
        /// </summary>
        Task<IReadOnlyList<TreeIdentifier>> GetEligibleRebuildsAsync(
            TimeSpan debounceWindow, CancellationToken ct = default);

        /// <summary>
        /// Attempts to claim a request for building. Sets Status to Building,
        /// generates and assigns a new BuildId, sets StartedAt.
        /// Returns the BuildId if successfully claimed, null if the request
        /// is no longer Pending (another job instance got there first).
        /// </summary>
        Task<Guid?> TryClaimRequestAsync(TreeIdentifier tree, CancellationToken ct = default);

        /// <summary>
        /// Checks whether the request is still in Building status.
        /// Returns false if it has been reset to Pending (meaning a new
        /// structural change arrived mid-build and the build should be abandoned).
        /// Called at checkpoints during the rebuild.
        /// </summary>
        Task<bool> IsStillBuildingAsync(TreeIdentifier tree, CancellationToken ct = default);

        /// <summary>
        /// Promotes the completed build to live. Updates ComplianceTreeBuild
        /// to point at the new BuildId, deletes old build's nodes,
        /// and sets the request status to Complete (or removes the request row).
        /// </summary>
        Task PromoteBuildAsync(TreeIdentifier tree, Guid newBuildId, CancellationToken ct = default);

        /// <summary>
        /// Cleans up after an abandoned or failed build. Deletes any nodes
        /// written under the given BuildId and resets the request to Pending
        /// so it gets picked up on the next poll cycle.
        /// </summary>
        Task AbandonBuildAsync(TreeIdentifier tree, Guid buildId, CancellationToken ct = default);
    }
}
