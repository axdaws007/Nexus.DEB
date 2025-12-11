using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Api.GraphQL
{
    public class HasDraftStandardVersionsDataLoader : BatchDataLoader<Guid, bool>
    {
        private readonly IDebService _debService;
        private readonly ILogger<HasDraftStandardVersionsDataLoader> _logger;

        public HasDraftStandardVersionsDataLoader(IDebService debService,
            ILogger<HasDraftStandardVersionsDataLoader> logger,
            IBatchScheduler batchScheduler,
            DataLoaderOptions? options = null)
            : base(batchScheduler, options)
        {
            _debService = debService;
            _logger = logger;
        }

        protected override async Task<IReadOnlyDictionary<Guid, bool>> LoadBatchAsync(
            IReadOnlyList<Guid> keys,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Batching active standard versions request for {Count} entities: {EntityIds}",
                    keys.Count,
                    string.Join(", ", keys.Take(5)) + (keys.Count > 5 ? "..." : ""));

                // Call the service with all entity IDs at once
                var statuses = await _debService.HasOtherDraftStandardVersionsForStandardsAsync(
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

                throw ex;
            }
        }

    }
}
