using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ITransitionSideEffect
    {
        /// <summary>
        /// Unique name matching PAWS configuration (e.g., "UpdateCompliance")
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Execution order (lower runs first). Default is 100.
        /// </summary>
        int Order => 100;

        /// <summary>
        /// Executes side effect. Errors logged but don't affect transition.
        /// </summary>
        Task<Result> ExecuteAsync(
            TransitionSideEffectContext context,
            CancellationToken cancellationToken = default);
    }

}
