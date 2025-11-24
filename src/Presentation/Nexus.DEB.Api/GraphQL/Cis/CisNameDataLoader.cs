using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Api.GraphQL
{
    //public class CisNameDataLoader : BatchDataLoader<Guid, string?>
    //{
    //    private readonly ICisService _cisService;
    //    private readonly ILogger<CisNameDataLoader> _logger;

    //    public CisNameDataLoader(
    //        ICisService cisService,
    //        ILogger<CisNameDataLoader> logger,
    //        IBatchScheduler batchScheduler,
    //        DataLoaderOptions? options = null)
    //        : base(batchScheduler, options)
    //    {
    //        _cisService = cisService;
    //        _logger = logger;
    //    }

    //    protected override async Task<IReadOnlyDictionary<Guid, string?>> LoadBatchAsync(
    //        IReadOnlyList<Guid> keys,
    //        CancellationToken cancellationToken)
    //    {
    //        try
    //        {
    //            _logger.LogInformation(
    //                "Batching workflow status request for {Count} entities: {EntityIds}",
    //                keys.Count,
    //                string.Join(", ", keys.Take(5)) + (keys.Count > 5 ? "..." : ""));

    //            // Call the service with all entity IDs at once
    //            var names = await _cisService.GetNamesByIdsAsync(
    //                keys.ToList(),
    //                cancellationToken);

    //            _logger.LogInformation(
    //                "Successfully retrieved {Count} CIS users from batch request",
    //                names.Count);

    //            // Return the dictionary
    //            return names;
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex,
    //                "Error fetching batch CIS users for {Count} IDs",
    //                keys.Count);

    //            // Return null for all keys if the batch fails
    //            // Alternatively, you could return partial results or throw
    //            return keys.ToDictionary(key => key, key => (string?)null);
    //        }
    //    }
    //}
}
