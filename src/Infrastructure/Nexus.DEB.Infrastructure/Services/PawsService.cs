using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

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

        public async Task<ICollection<PendingActivity>?> GetPendingActivitiesAsync(Guid entityId, Guid workflowId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await SendAuthenticatedRequestAsync<ICollection<PendingActivity>>(
                HttpMethod.Get,
                $"api/PAWSClient/GetPendingActivities?entityID={entityId}&workflowID={workflowId}",
                operationName: $"GetPendingActivities for {entityId} entity and {workflowId} workflow");

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching pending activities");
                throw;
            }
        }

        public async Task<DestinationActivity?> GetDestinationActivitiesAsync(int stepId, int triggerStatusId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await SendAuthenticatedRequestAsync<DestinationActivity>(
                HttpMethod.Get,
                $"api/PAWSClient/GetDestinationActivities?stepID={stepId}&triggerStateID={triggerStatusId}",
                operationName: $"GetDestinationActivities for {stepId} stepId and {triggerStatusId} triggerStatusId");

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching destination activities");
                throw;
            }
        }

        public async Task<TransitionConfigurationInfo?> GetTransitionByTriggerAsync(int sourceActivityId, int triggerStatusId, CancellationToken cancellationToken = default)
        {
            TransitionConfigurationInfo info = new TransitionConfigurationInfo()
            {
                ActivityTransitionId = sourceActivityId,
                SourceActivityId = sourceActivityId,
                DestinationActivityId = sourceActivityId + 1,
                TriggerStatusId = triggerStatusId,
                SideEffectNames = new List<string>(),
                ValidatorNames = new List<string>() { "AllTasksClosed" }
            };

            return await Task.FromResult(info);
        }

        public async Task<ICollection<WorkflowPseudoState>?> GetPseudoStatesByWorkflowAsync(Guid workflowId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await SendAuthenticatedRequestAsync<ICollection<WorkflowPseudoState>>(
                HttpMethod.Get,
                $"api/PAWSClient/GetWorkflowPseudoStates?workflowID={workflowId}",
                operationName: $"GetWorkflowPseudoStates for {workflowId} workflow");

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching pending activities");
                throw;
            }

        }

        public async Task<bool> CreateWorkflowInstanceAsync(Guid workflowID, Guid entityId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Create request DTO
                var request = new CreateWorkflowInstanceRequest
                {
                    WorkflowID = workflowID,
                    EntityID = entityId
                };

                // Create JSON content (JsonOptions from base class)
                var content = JsonContent.Create(request, options: JsonOptions);

                // Use base class method - it gets the auth cookie from HttpContext automatically!
                var response = await SendAuthenticatedRequestAsync<bool>(
                    HttpMethod.Post,
                    "api/PAWSClient/CreateWorkflowInstance",
                    operationName: $"CreateWorkflowInstance for {workflowID} workflowId and {entityId} entityId",
                    content: content);

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching batch workflow statuses");
                throw;
            }
        }

        public async Task<bool> ApproveStepAsync(
            Guid workflowId, 
            Guid entityId, 
            int stepId,
            int statusId,
            int[] destinationActivityId,
            string? comments = null,
            Guid? onBehalfOfId = null,
            string? password = null,
            Guid[]? defaultOwnerIds = null, 
            CancellationToken cancellationToken = default)
        {
            if (defaultOwnerIds == null)
            {
                defaultOwnerIds = new Guid[1];
                defaultOwnerIds[0] = Guid.Empty;
            }

            if (comments == null) comments = string.Empty;
            if (password == null) password = string.Empty;
            if (onBehalfOfId == null) onBehalfOfId = Guid.Empty;

            try
            {
                // Create request DTO
                var request = new ApproveStepRequest
                {
                    WorkflowID = workflowId,
                    EntityID = entityId,
                    Comments = comments,
                    DefaultOwnerID = defaultOwnerIds,
                    DestinationActivityID = destinationActivityId,
                    OnBehalfOfID = onBehalfOfId,
                    Password = password,
                    SelectedStateID = statusId,
                    StepID = stepId
                };

                // Create JSON content (JsonOptions from base class)
                var content = JsonContent.Create(request, options: JsonOptions);

                // Use base class method - it gets the auth cookie from HttpContext automatically!
                var response = await SendAuthenticatedRequestAsync<ApproveStepResponse>(
                    HttpMethod.Post,
                    "api/PAWSClient/ApproveStep",
                    operationName: $"ApproveStep for {workflowId} workflowId and {entityId} entityId",
                    content: content);

                if (response == null) return false;

                return response.Success;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching batch workflow statuses");
                throw;
            }
        }

        public async Task<WorkflowHistory>? GetWorkflowHistoryAsync(
            Guid workflowId,
            Guid entityId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await SendAuthenticatedRequestAsync<WorkflowHistory>(
                HttpMethod.Get,
                $"api/PAWSClient/GetHistory?entityID={entityId}&workflowID={workflowId}",
                operationName: $"GetHistory for {entityId} entity and {workflowId} workflow");

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching workflow history");
                throw;
            }
        }

        public async Task<string?> GetWorkflowDiagramHtmlAsync(Guid workflowId, Guid entityId, CancellationToken cancellationToken)
        {
            try
            {
                var requestUri = $"PAWSDiagramViewer/RenderPAWSDiagramViewer?ProcessTemplateID={workflowId}&EntityID={entityId}";

                Logger.LogInformation("Getting workflow diagram HTML for entity {EntityId}", entityId);

                var request = CreateAuthenticatedRequest(HttpMethod.Get, requestUri);
                var response = await HttpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Logger.LogWarning("Workflow diagram not found for entity {EntityId}", entityId);
                    return null;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    Logger.LogWarning("Access denied to workflow diagram for entity {EntityId}: {StatusCode}",
                        entityId, (int)response.StatusCode);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync();

                // Rewrite image src URLs to point to our BFF proxy instead of the legacy API
                html = RewriteImageUrls(html);

                Logger.LogInformation("Successfully retrieved workflow diagram HTML for entity {EntityId} ({Length} chars)",
                    entityId, html.Length);

                return html;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving workflow diagram HTML for entity {EntityId}", entityId);
                throw;
            }
        }

        /// <summary>
        /// Gets the actual workflow diagram image from PAWS API.
        /// </summary>
        public async Task<byte[]?> GetWorkflowDiagramImageAsync(string cacheKey, CancellationToken cancellationToken)
        {
            try
            {
                var requestUri = $"PAWSDiagramViewer/GetCachedPAWSDiagram/{cacheKey}";

                Logger.LogInformation("Getting workflow diagram image for cache key {CacheKey}", cacheKey);

                var request = CreateAuthenticatedRequest(HttpMethod.Get, requestUri);
                var response = await HttpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Logger.LogWarning("Workflow diagram image not found for cache key {CacheKey}", cacheKey);
                    return null;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    Logger.LogWarning("Access denied to workflow diagram image for cache key {CacheKey}: {StatusCode}",
                        cacheKey, (int)response.StatusCode);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var imageBytes = await response.Content.ReadAsByteArrayAsync();

                Logger.LogInformation("Successfully retrieved workflow diagram image for cache key {CacheKey} ({Size} bytes)",
                    cacheKey, imageBytes.Length);

                return imageBytes;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving workflow diagram image for cache key {CacheKey}", cacheKey);
                throw;
            }
        }

        /// <summary>
        /// Rewrites image src URLs in the HTML to point to our BFF proxy.
        /// Example: /Nexus.PAWS.WebAPI/PAWSDiagramViewer/GetCachedPAWSDiagram/abc123?t=123456
        /// Becomes: /api/workflow-diagrams/images/abc123?t=123456
        /// </summary>
        private string RewriteImageUrls(string html)
        {
            // Pattern to match the PAWS API image URLs
            var pattern = @"src=""(/Nexus\.PAWS\.WebAPI)?/PAWSDiagramViewer/GetCachedPAWSDiagram/([^""?]+)(\?[^""]*)?""";

            // Replace with our BFF proxy URL
            var rewritten = Regex.Replace(html, pattern, match =>
            {
                var cacheKey = match.Groups[2].Value;
                var queryString = match.Groups[3].Value;
                return $@"src=""/api/workflow-diagrams/images/{cacheKey}{queryString}""";
            });

            return rewritten;
        }
    }

}
