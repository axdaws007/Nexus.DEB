using Microsoft.Extensions.Caching.Memory;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Compliance;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Infrastructure.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Nexus.DEB.Api.Restful
{
    // Add to TestDataEndpoints.cs or a new ComplianceAdminEndpoints.cs

    public static class ComplianceAdminEndpoints
    {
        public static void MapComplianceAdminEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/compliance")
                .RequireAuthorization()
                .WithTags("Compliance Admin");

            // Rebuild all trees 
            group.MapPost("/rebuild/all",
                async (
                    IDebService debService,
                    IComplianceTreeRecalculator recalculator,
                    ILogger<Program> logger,
                    CancellationToken cancellationToken) =>
                {
                    logger.LogInformation("Manual compliance tree rebuild requested for all standard versions");

                    var standardVersionIds = debService.GetStandardVersions().Select(x => x.EntityId).ToList();

                    foreach(var standardVersionId in standardVersionIds)
                    {
                        logger.LogInformation(
                        "Manual compliance tree rebuild requested for StandardVersion {Id}",
                        standardVersionId);

                        try
                        {
                            await recalculator.RebuildAllTreesForStandardVersionDirectAsync(
                            standardVersionId, cancellationToken);

                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex,
                            "Failed to rebuild compliance trees for StandardVersion {Id}",
                            standardVersionId);

                            return Results.Problem(
                            title: $"Compliance Tree Rebuild Failed for {standardVersionId}",
                            detail: ex.Message,
                            statusCode: StatusCodes.Status500InternalServerError);
                        }
                    }

                    return Results.Ok(new
                    {
                        message = $"Compliance trees rebuilt for all StandardVersions"
                    });
                });

            // Rebuild all trees for a specific Standard Version (across all Scopes)
            group.MapPost("/rebuild/standard-version/{standardVersionId:guid}",
                async (
                    Guid standardVersionId,
                    IComplianceTreeRecalculator recalculator,
                    ILogger<Program> logger,
                    CancellationToken cancellationToken) =>
                {
                    logger.LogInformation(
                    "Manual compliance tree rebuild requested for StandardVersion {Id}",
                    standardVersionId);

                    try
                    {
                        await recalculator.RebuildAllTreesForStandardVersionDirectAsync(
                        standardVersionId, cancellationToken);

                        return Results.Ok(new
                        {
                            message = $"Compliance trees rebuilt for StandardVersion {standardVersionId}",
                            standardVersionId
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex,
                        "Failed to rebuild compliance trees for StandardVersion {Id}",
                        standardVersionId);

                        return Results.Problem(
                        title: "Compliance Tree Rebuild Failed",
                        detail: ex.Message,
                        statusCode: StatusCodes.Status500InternalServerError);
                    }
                });

            // Rebuild a specific tree (Standard Version + Scope)
            group.MapPost("/rebuild/tree/{standardVersionId:guid}/{scopeId:guid}",
                async (
                    Guid standardVersionId,
                    Guid scopeId,
                    IComplianceTreeRecalculator recalculator,
                    ILogger<Program> logger,
                    CancellationToken cancellationToken) =>
                {
                    logger.LogInformation(
                    "Manual compliance tree rebuild requested for SV={StandardVersionId} Scope={ScopeId}",
                    standardVersionId, scopeId);

                    try
                    {
                        var tree = new TreeIdentifier(standardVersionId, scopeId);
                        await recalculator.RebuildTreeDirectAsync(tree, cancellationToken);

                        return Results.Ok(new
                        {
                            message = $"Compliance tree rebuilt",
                            standardVersionId,
                            scopeId
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex,
                        "Failed to rebuild compliance tree for SV={StandardVersionId} Scope={ScopeId}",
                        standardVersionId, scopeId);

                        return Results.Problem(
                        title: "Compliance Tree Rebuild Failed",
                        detail: ex.Message,
                        statusCode: StatusCodes.Status500InternalServerError);
                    }
                });

            // Get tree stats (useful for verifying a rebuild worked)
            group.MapGet("/tree/{standardVersionId:guid}/{scopeId:guid}/stats",
                async (
                    Guid standardVersionId,
                    Guid scopeId,
                    IDebService debService,
                    CancellationToken cancellationToken) =>
                {
                    var tree = new TreeIdentifier(standardVersionId, scopeId);
                    var buildInfo = await debService.GetCurrentLiveBuildInformationAsync(tree, cancellationToken);

                    var nodes = await debService.GetComplianceTreeAsync(tree, buildInfo!.LiveBuildId, cancellationToken);

                    var stats = new
                    {
                        standardVersionId,
                        scopeId,
                        totalNodes = nodes.Count,
                        byNodeType = nodes
                        .GroupBy(n => n.NodeType)
                        .ToDictionary(g => g.Key, g => g.Count()),
                        byComplianceState = nodes
                        .Where(n => n.ComplianceStateID.HasValue)
                        .GroupBy(n => n.ComplianceState?.Name ?? "Unknown")
                        .ToDictionary(g => g.Key, g => g.Count()),
                        nodesWithLabels = nodes
                        .Where(n => n.ComplianceStateLabel != null)
                        .Select(n => new { n.NodeType, n.EntityID, n.ComplianceStateLabel })
                        .ToList(),
                        rootNode = nodes
                        .Where(n => n.NodeType == ComplianceNodeTypes.StandardVersion)
                        .Select(n => new
                        {
                            n.ComplianceStateID,
                            complianceState = n.ComplianceState?.Name,
                            n.ComplianceStateLabel,
                            n.TotalRequirementCount,
                            n.TotalSectionCount,
                            summaries = n.Summaries.Select(s => new
                            {
                                s.ChildNodeType,
                                complianceState = s.ComplianceState?.Name,
                                s.Count
                            })
                        })
                        .FirstOrDefault()
                    };

                    return Results.Ok(stats);
                });

            // Invalidate compliance engine caches
            group.MapPost("/cache/invalidate",
                (IMemoryCache cache, ILogger<Program> logger) =>
                {
                    // Remove all compliance engine cache entries
                    cache.Remove("ComplianceEngine:ComplianceStateMappings");
                    cache.Remove("ComplianceEngine:BubbleUpRules");
                    cache.Remove("ComplianceEngine:NodeDefaults");
                    cache.Remove("ComplianceEngine:ComplianceStates");

                    logger.LogInformation("Compliance engine caches invalidated");

                    return Results.Ok(new { message = "Compliance engine caches invalidated" });
                });
        }
    }
}
