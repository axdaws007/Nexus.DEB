using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ITransitionValidator
    {
        /// <summary>
        /// Unique name used in PAWS configuration (e.g., "AllTasksClosed")
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Validates the transition. Return failure to block it.
        /// </summary>
        /// <param name="context">Information about the transition being validated</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success if validation passes, Failure with errors if not</returns>
        Task<Result> ValidateAsync(
            TransitionValidationContext context,
            CancellationToken cancellationToken = default);
    }
}
