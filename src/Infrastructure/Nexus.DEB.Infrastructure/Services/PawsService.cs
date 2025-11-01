using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Infrastructure.Services
{
    /// <summary>
    /// Service for interacting with the Workflow API (PAWS - Parallel Approval Workflow System).
    /// 
    /// SECURITY: Authentication cookies are retrieved from HttpContext (request-scoped),
    /// ensuring cookies are NEVER shared across different user requests.
    /// </summary>
    public class PawsService : LegacyApiServiceBase<PawsService>, IPawsService
    {
        protected override string HttpClientName => "WorkflowApi";

        public PawsService(
            IHttpClientFactory httpClientFactory,
            ILogger<PawsService> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
            : base(httpClientFactory, logger, httpContextAccessor, configuration)
        {
        }


        /// <summary>
        /// Batch method - gets statuses for multiple entities in ONE API call.
        /// This is called by the DataLoader with all accumulated entity IDs.
        /// Authentication cookie is automatically retrieved from the current HTTP context.
        /// </summary>
        public async Task<IReadOnlyDictionary<Guid, string?>> GetStatusesForEntitiesAsync(
            List<Guid> entityIds,
            CancellationToken cancellationToken = default)
        {
            Logger.LogInformation(
                "Fetching workflow statuses for {Count} entities in batch",
                entityIds.Count);

            // MOCK IMPLEMENTATION - Replace with real API call when available
            await Task.Delay(1, cancellationToken); // Simulate async operation

            var result = entityIds.ToDictionary(
                id => id,
                id => (string?)"TBD" // Mock status
            );

            Logger.LogInformation(
                "Successfully fetched {Count} workflow statuses",
                result.Count);

            return result;

            /* REAL IMPLEMENTATION (uncomment when API is available):
            
            try
            {
                // Create request DTO
                var request = new WorkflowStatusBatchRequest
                {
                    EntityIds = entityIds
                };

                // Create JSON content (JsonOptions from base class)
                var content = JsonContent.Create(request, options: JsonOptions);

                // Use base class method - it gets the auth cookie from HttpContext automatically!
                var response = await SendAuthenticatedRequestAsync<WorkflowStatusBatchResponse>(
                    HttpMethod.Post,
                    "api/Workflow/BatchStatus",
                    operationName: $"GetBatchWorkflowStatuses for {entityIds.Count} entities",
                    content: content);

                if (response?.Statuses == null)
                {
                    Logger.LogWarning("API returned null response for batch status request");
                    return entityIds.ToDictionary(id => id, id => (string?)null);
                }

                // Convert API response to dictionary
                var result = new Dictionary<Guid, string?>();
                foreach (var status in response.Statuses)
                {
                    result[status.EntityId] = status.WorkflowStatus;
                }

                // Add null entries for any entity IDs that weren't returned
                foreach (var entityId in entityIds)
                {
                    if (!result.ContainsKey(entityId))
                    {
                        result[entityId] = null;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching batch workflow statuses");
                throw;
            }
            */
        }
    }
}