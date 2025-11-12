using Microsoft.AspNetCore.Mvc;
using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Api.Restful
{
    public static class WorkflowDiagramEndpoints
    {
        public static void MapWorkflowDiagramEndpoints(this WebApplication app)
        {
            var diagramGroup = app.MapGroup("/api/workflow-diagrams")
                .WithTags("Workflow Diagrams")
                .WithOpenApi();

            // Endpoint to get the HTML with image map
            diagramGroup.MapGet("/{entityId}", GetWorkflowDiagramHtml)
                .RequireAuthorization()
                .WithName("GetWorkflowDiagramHtml")
                .WithSummary("Get workflow diagram HTML with clickable image map")
                .Produces<string>(StatusCodes.Status200OK, contentType: "text/html")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound);

            // Endpoint to proxy the actual image
            diagramGroup.MapGet("/images/{cacheKey}", GetWorkflowDiagramImage)
                .RequireAuthorization()
                .WithName("GetWorkflowDiagramImage")
                .WithSummary("Get workflow diagram image (proxied from legacy API)")
                .Produces<FileResult>(StatusCodes.Status200OK, contentType: "image/png")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound);
        }

        private static async Task<IResult> GetWorkflowDiagramHtml(
            [FromRoute] Guid entityId,
            [FromServices] IDebService debService,
            [FromServices] IPawsService pawsService,
            [FromServices] IConfiguration configuration,
            [FromServices] ILogger<Program> logger,
            CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Workflow diagram HTML request received for entity: {EntityId}", entityId);

                var moduleIdString = configuration["Modules:DEB"] ?? throw new InvalidOperationException("Modules:DEB not configured in appsettings");

                if (!Guid.TryParse(moduleIdString, out var moduleId))
                {
                    throw new InvalidOperationException("Modules:DEB must be a valid GUID");
                }

                var entity = await debService.GetEntityHeadAsync(entityId, cancellationToken);

                if (entity == null)
                {
                    throw new InvalidOperationException("EntityID could not be identified");
                }

                var workflowId = await debService.GetWorkflowIdAsync(moduleId, entity.EntityTypeTitle, cancellationToken);

                if (workflowId.HasValue == false)
                {
                    throw new InvalidOperationException("WorkflowID could not be identified");
                }

                var html = await pawsService.GetWorkflowDiagramHtmlAsync(workflowId.Value, entityId, cancellationToken);

                if (string.IsNullOrEmpty(html))
                {
                    logger.LogWarning("Workflow diagram HTML not found for entity: {EntityId}", entityId);
                    return Results.NotFound(new { message = $"Workflow diagram not found for entity '{entityId}'" });
                }

                logger.LogInformation("Returning workflow diagram HTML for entity {EntityId} ({Length} chars)",
                    entityId, html.Length);

                return Results.Content(html, "text/html");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving workflow diagram HTML for entity {EntityId}", entityId);
                return Results.Problem(
                    title: "Workflow Diagram Retrieval Failed",
                    detail: "An error occurred while retrieving the workflow diagram. Please try again.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task<IResult> GetWorkflowDiagramImage(
            [FromRoute] string cacheKey,
            [FromServices] IPawsService pawsService,
            [FromServices] ILogger<Program> logger,
            CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Workflow diagram image request received for cache key: {CacheKey}", cacheKey);

                var imageBytes = await pawsService.GetWorkflowDiagramImageAsync(cacheKey, cancellationToken);

                if (imageBytes == null || imageBytes.Length == 0)
                {
                    logger.LogWarning("Workflow diagram image not found for cache key: {CacheKey}", cacheKey);
                    return Results.NotFound(new { message = $"Workflow diagram image not found for cache key '{cacheKey}'" });
                }

                logger.LogInformation("Returning workflow diagram image for cache key {CacheKey} ({Size} bytes)",
                    cacheKey, imageBytes.Length);

                return Results.File(
                    imageBytes,
                    contentType: "image/png");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving workflow diagram image for cache key {CacheKey}", cacheKey);
                return Results.Problem(
                    title: "Image Retrieval Failed",
                    detail: "An error occurred while retrieving the workflow diagram image. Please try again.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
