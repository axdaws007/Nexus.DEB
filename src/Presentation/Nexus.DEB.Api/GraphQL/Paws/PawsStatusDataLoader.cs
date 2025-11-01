using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Api.GraphQL.Paws
{
    public class PawsStatusDataLoader : BatchDataLoader<Guid, string?>
    {
        private readonly IPawsService _pawsService;
        private readonly ILogger<PawsStatusDataLoader> _logger;

        public PawsStatusDataLoader(
            IPawsService pawsService,
            ILogger<PawsStatusDataLoader> logger,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _pawsService = pawsService;
            _logger = logger;
        }

        /// <summary>
        /// This method is called once per batch with all the entity IDs that were requested.
        /// It fetches all statuses in a single API call and returns them in the correct order.
        /// </summary>
        /// <param name="keys">All entity IDs that need workflow status</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dictionary mapping entity ID to workflow status</returns>
        protected override async Task<IReadOnlyDictionary<Guid, string?>> LoadBatchAsync(
            IReadOnlyList<Guid> keys,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Batching workflow status request for {Count} entities: {EntityIds}",
                    keys.Count,
                    string.Join(", ", keys.Take(5)) + (keys.Count > 5 ? "..." : ""));

                // Call the service with all entity IDs at once
                var statuses = await _pawsService.GetStatusesForEntitiesAsync(
                    keys.ToList(),
                    cancellationToken);

                _logger.LogInformation(
                    "Successfully retrieved {Count} workflow statuses from batch request",
                    statuses.Count);

                // Return the dictionary
                return statuses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error fetching batch workflow statuses for {Count} entities",
                    keys.Count);

                // Return null for all keys if the batch fails
                // Alternatively, you could return partial results or throw
                return keys.ToDictionary(key => key, key => (string?)null);
            }
        }
    }
}
