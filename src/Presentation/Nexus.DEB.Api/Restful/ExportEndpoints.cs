using CsvHelper;
using CsvHelper.Configuration;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Api.Restful.Maps;
using Nexus.DEB.Application.Common.Extensions;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Models.Common;
using Nexus.DEB.Infrastructure.Services;
using System.Globalization;
using System.Text;

namespace Nexus.DEB.Api.Restful
{
    public static class ExportEndpoints
    {
        public static void MapExportEndpoints(this WebApplication app)
        {
            var exportGroup = app.MapGroup("/api/export")
                .WithTags("Export")
                .WithOpenApi();

            exportGroup.MapPost("/standard-versions-csv", ExportStandardVersionsAsCsv)
                .RequireAuthorization()
                .WithName("ExportStandardVersionsAsCsv")
                .WithSummary("Export standard versions as CSV file")
                .Produces<FileResult>(StatusCodes.Status200OK, contentType: "text/csv")
                .Produces(StatusCodes.Status401Unauthorized);

            exportGroup.MapPost("/tasks-csv", ExportTasksAsCsv)
                .RequireAuthorization()
                .WithName("ExportTasksAsCsv")
                .WithSummary("Export tasks as CSV file")
                .Produces<FileResult>(StatusCodes.Status200OK, contentType: "text/csv")
                .Produces(StatusCodes.Status401Unauthorized);

            exportGroup.MapPost("/scopes-csv", ExportScopesAsCsv)
                .RequireAuthorization()
                .WithName("ExportScopesAsCsv")
                .WithSummary("Export scopes as CSV file")
                .Produces<FileResult>(StatusCodes.Status200OK, contentType: "text/csv")
                .Produces(StatusCodes.Status401Unauthorized);

            exportGroup.MapPost("/requirements-csv", ExportRequirementsAsCsv)
                .RequireAuthorization()
                .WithName("ExportRequirementsAsCsv")
                .WithSummary("Export requirements as CSV file")
                .Produces<FileResult>(StatusCodes.Status200OK, contentType: "text/csv")
                .Produces(StatusCodes.Status401Unauthorized);

            exportGroup.MapPost("/statements-csv", ExportStatementsAsCsv)
                .RequireAuthorization()
                .WithName("ExportStatementsAsCsv")
                .WithSummary("Export statements as CSV file")
                .Produces<FileResult>(StatusCodes.Status200OK, contentType: "text/csv")
                .Produces(StatusCodes.Status401Unauthorized);

            exportGroup.MapPost("/mywork-csv", ExportMyWorkAsCsv)
                .RequireAuthorization()
                .WithName("ExportMyWorkAsCsv")
                .WithSummary("Export my work as CSV file")
                .Produces<FileResult>(StatusCodes.Status200OK, contentType: "text/csv")
                .Produces(StatusCodes.Status401Unauthorized);
        }

        private static async Task<IResult> ExportStandardVersionsAsCsv(
            [FromBody] StandardVersionSummaryFilters? filters,
            [FromServices] IDebService debService,
            [FromServices] ILogger<Program> logger,
            [FromServices] IAuditService auditService,
            [FromServices] ICurrentUserService currentUserService,
            CancellationToken cancellationToken)
        {
            return await ExportToCsvAsync(
                entityName: EntityTypes.StandardVersion,
                getDataQuery: () => debService.GetStandardVersionsForExport(filters),
                fileNamePrefix: "standard-versions",
                registerClassMap: csv => csv.Context.RegisterClassMap<StandardVersionExportMap>(),
                filters: filters,
                logger: logger,
                auditService: auditService,
                currentUserService: currentUserService,
                cancellationToken: cancellationToken);
        }

        private static async Task<IResult> ExportTasksAsCsv(
            [FromBody] TaskSummaryFilters? filters,
            [FromServices] IDebService debService,
            [FromServices] ILogger<Program> logger,
            [FromServices] IAuditService auditService,
            [FromServices] ICurrentUserService currentUserService,
            CancellationToken cancellationToken)
        {
            return await ExportToCsvAsync(
                entityName: EntityTypes.Task,
                getDataQuery: () => debService.GetTasksForExport(filters),
                fileNamePrefix: "tasks",
                registerClassMap: csv => csv.Context.RegisterClassMap<TaskExportMap>(),
                filters: filters,
                logger: logger,
                auditService: auditService,
                currentUserService: currentUserService,
                cancellationToken: cancellationToken);
        }

        private static async Task<IResult> ExportScopesAsCsv(
            [FromServices] IDebService debService,
            [FromServices] ILogger<Program> logger,
            [FromServices] IAuditService auditService,
            [FromServices] ICurrentUserService currentUserService,
            CancellationToken cancellationToken)
        {
            return await ExportToCsvAsync(
                entityName: EntityTypes.Scope,
                getDataQuery: () => debService.GetScopesForExport(),
                fileNamePrefix: "scopes",
                registerClassMap: csv => csv.Context.RegisterClassMap<ScopeExportMap>(),
                filters: null,
                logger: logger,
                auditService: auditService,
                currentUserService: currentUserService,
                cancellationToken: cancellationToken);
        }

        private static async Task<IResult> ExportRequirementsAsCsv(
            [FromBody] RequirementSummaryFilters? filters,
            [FromServices] IDebService debService,
            [FromServices] ILogger<Program> logger,
            [FromServices] IAuditService auditService,
            [FromServices] ICurrentUserService currentUserService,
            CancellationToken cancellationToken)
        {
            return await ExportToCsvAsync(
                entityName: EntityTypes.Requirement,
                getDataQuery: () => debService.GetRequirementsForExport(filters),
                fileNamePrefix: "requirements",
                registerClassMap: csv => csv.Context.RegisterClassMap<RequirementExportMap>(),
                filters: filters,
                logger: logger,
                auditService: auditService,
                currentUserService: currentUserService,
                cancellationToken: cancellationToken);
        }

        private static async Task<IResult> ExportStatementsAsCsv(
            [FromBody] StatementSummaryFilters? filters,
            [FromServices] IDebService debService,
            [FromServices] ILogger<Program> logger,
            [FromServices] IAuditService auditService,
            [FromServices] ICurrentUserService currentUserService,
            CancellationToken cancellationToken)
        {
            return await ExportToCsvAsync(
                entityName: EntityTypes.SoC,
                getDataQuery: () => debService.GetStatementsForExport(filters),
                fileNamePrefix: "statements",
                registerClassMap: csv => csv.Context.RegisterClassMap<StatementExportMap>(),
                filters: filters,
                logger: logger,
                auditService: auditService,
                currentUserService: currentUserService,
                cancellationToken: cancellationToken);
        }

        private static async Task<IResult> ExportMyWorkAsCsv(
            [FromBody] MyWorkDetailFilters? providedFilters,
            [FromServices] IDebService debService,
            [FromServices] ILogger<Program> logger,
            [FromServices] IAuditService auditService,
            [FromServices] ICbacService cbacService,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IApplicationSettingsService applicationSettingsService,
            CancellationToken cancellationToken)
        {
            DebHelper.MyWork.FilterTypes.RequiringProgression.Validator.ValidateOrThrow(providedFilters.RequiringProgressionBy);
            DebHelper.MyWork.FilterTypes.CreatedBy.Validator.ValidateOrThrow(providedFilters.CreatedBy);
            DebHelper.MyWork.FilterTypes.OwnedBy.Validator.ValidateOrThrow(providedFilters.OwnedBy);

            var moduleId = applicationSettingsService.GetModuleId("DEB");
            var workflowId = await debService.GetWorkflowIdAsync(moduleId, providedFilters.EntityTypeTitle, cancellationToken);

            List<Guid> roleIds;

            var postId = currentUserService.PostId;
            var roles = await cbacService.GetRolesForPostAsync(postId);

            if (roles == null)
                roleIds = [];
            else
                roleIds = [.. roles.Select(x => x.RoleID)];

            var supplementedFilters = providedFilters.Adapt<MyWorkDetailSupplementedFilters>();

            supplementedFilters.WorkflowId = workflowId.Value;
            supplementedFilters.PostId = currentUserService.PostId;
            supplementedFilters.RoleIds = roleIds;

            return await ExportToCsvAsync(
                entityName: "MyWork",
                getDataQuery: () => debService.GetMyWorkDetailItems(supplementedFilters),
                fileNamePrefix: "mywork",
                registerClassMap: csv => csv.Context.RegisterClassMap<MyWorkExportMap>(),
                filters: supplementedFilters,
                logger: logger,
                auditService: auditService,
                currentUserService: currentUserService,
                cancellationToken: cancellationToken);
        }


        /// <summary>
        /// Generic helper method to export data to CSV format
        /// </summary>
        /// <typeparam name="TData">The type of data being exported</typeparam>
        /// <param name="entityName">Name of the entity for logging purposes (e.g., "Tasks", "StandardVersions")</param>
        /// <param name="getDataQuery">Function that returns an IQueryable for the data to export</param>
        /// <param name="fileNamePrefix">Prefix for the generated filename (e.g., "tasks", "standard-versions")</param>
        /// <param name="registerClassMap">Action to register the CsvHelper class map for custom column mapping</param>
        /// <param name="logger">Logger instance for diagnostic information</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private static async Task<IResult> ExportToCsvAsync<TData>(
            string entityName,
            Func<IQueryable<TData>> getDataQuery,
            string fileNamePrefix,
            Action<CsvWriter> registerClassMap,
            object? filters,
            ILogger logger,
            IAuditService auditService,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Export request received for {EntityName}", entityName);

                var userDetails = await currentUserService.GetUserDetailsAsync();

                // Execute query to get data
                var query = getDataQuery();

                // Handle both EF-backed queryables (can use ToListAsync) 
                // and in-memory queryables (must use ToList)
                List<TData> data;
                if (query is IAsyncEnumerable<TData>)
                {
                    // EF Core queryable - use async
                    data = await query.ToListAsync(cancellationToken);
                }
                else
                {
                    // In-memory queryable (e.g., from stored procedure) - use sync
                    data = query.ToList();
                }

                logger.LogInformation("Retrieved {Count} {EntityName} for export", data.Count, entityName);

                // Generate CSV using CsvHelper
                using var memoryStream = new MemoryStream();
                using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                });

                // Register the class map for custom column headers/mappings
                registerClassMap(csv);

                await csv.WriteRecordsAsync(data, cancellationToken);
                await writer.FlushAsync();

                var csvBytes = memoryStream.ToArray();
                var fileName = $"{fileNamePrefix}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

                logger.LogInformation("Returning CSV file: {FileName} ({Size} bytes)",
                    fileName, csvBytes.Length);

                await auditService.DataExported(
                    null, 
                    entityName, 
                    $"CSV export of {entityName}. File name = {fileName}. File size = {csvBytes.Length} bytes", 
                    userDetails,
                    JsonElementExtensions.ToExportAuditData(
                        fileName: fileName,
                        fileContent: csvBytes,
                        recordCount: data.Count,
                        filters: filters,
                        includeFileContent: true)
                    );

                return Results.File(
                    csvBytes,
                    contentType: "text/csv",
                    fileDownloadName: fileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during {EntityName} CSV export", entityName);
                return Results.Problem(
                    title: "Export Failed",
                    detail: "An error occurred while generating the CSV export. Please try again.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
