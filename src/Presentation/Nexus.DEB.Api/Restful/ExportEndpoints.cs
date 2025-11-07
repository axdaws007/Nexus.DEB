using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Api.Restful.Maps;
using Nexus.DEB.Api.Restful.Models;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
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

            if (app.Environment.IsDevelopment())
            {
                var testGroup = app.MapGroup("/api/testdata")
                    .WithTags("TestData")
                    .WithOpenApi();

                testGroup.MapPost("/statements-and-tasks", GenerateStatementsAndTasks)
                    .RequireAuthorization()
                    .WithName("GenerateStatementsAndTasks")
                    .WithSummary("Generate sample statements and tasks")
                    .Produces(StatusCodes.Status200OK)
                    .Produces(StatusCodes.Status401Unauthorized);

            }
        }

        private static async Task<IResult> ExportStandardVersionsAsCsv(
            [FromBody] StandardVersionSummaryFilters? filters,
            [FromServices] IDebService debService,
            [FromServices] ILogger<Program> logger,
            CancellationToken cancellationToken)
        {
            return await ExportToCsvAsync(
                entityName: "StandardVersions",
                getDataQuery: () => debService.GetStandardVersionsForExport(filters),
                fileNamePrefix: "standard-versions",
                registerClassMap: csv => csv.Context.RegisterClassMap<StandardVersionExportMap>(),
                logger: logger,
                cancellationToken: cancellationToken);
        }

        private static async Task<IResult> ExportTasksAsCsv(
            [FromBody] TaskSummaryFilters? filters,
            [FromServices] IDebService debService,
            [FromServices] ILogger<Program> logger,
            CancellationToken cancellationToken)
        {
            return await ExportToCsvAsync(
                entityName: "Tasks",
                getDataQuery: () => debService.GetTasksForExport(filters),
                fileNamePrefix: "tasks",
                registerClassMap: csv => csv.Context.RegisterClassMap<TaskExportMap>(),
                logger: logger,
                cancellationToken: cancellationToken);
        }

        private static async Task<IResult> ExportScopesAsCsv(
            [FromServices] IDebService debService,
            [FromServices] ILogger<Program> logger,
            CancellationToken cancellationToken)
        {
            return await ExportToCsvAsync(
                entityName: "Scopes",
                getDataQuery: () => debService.GetScopesForExport(),
                fileNamePrefix: "scopes",
                registerClassMap: csv => csv.Context.RegisterClassMap<ScopeExportMap>(),
                logger: logger,
                cancellationToken: cancellationToken);
        }

        private static async Task<IResult> ExportRequirementsAsCsv(
            [FromBody] RequirementSummaryFilters? filters,
            [FromServices] IDebService debService,
            [FromServices] ILogger<Program> logger,
            CancellationToken cancellationToken)
        {
            return await ExportToCsvAsync(
                entityName: "Requirements",
                getDataQuery: () => debService.GetRequirementsForExport(filters),
                fileNamePrefix: "requirements",
                registerClassMap: csv => csv.Context.RegisterClassMap<RequirementExportMap>(),
                logger: logger,
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
            ILogger logger,
            CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Export request received for {EntityName}", entityName);

                // Execute query to get data
                var data = await getDataQuery()
                    .ToListAsync(cancellationToken);

                logger.LogInformation("Retrieved {Count} {EntityName} for export",
                    data.Count, entityName.ToLowerInvariant());

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

        private static async Task<IResult> GenerateStatementsAndTasks(
            [FromBody] StatementAndTasksParameters? parameters,
            [FromServices] IDebService debService,
            [FromServices] IPawsService pawsService,
            [FromServices] IConfiguration configuration,
            [FromServices] ILogger<Program> logger,
            CancellationToken cancellationToken)
        {
            var moduleIdString = configuration["Modules:DEB"] ?? throw new InvalidOperationException("Modules:DEB not configured in appsettings");

            if (!Guid.TryParse(moduleIdString, out var moduleId))
            {
                throw new InvalidOperationException("Modules:DEB must be a valid GUID");
            }

            var statementWorkflowId = await debService.GetWorkflowIdAsync(moduleId, EntityTypes.SoC, cancellationToken);
            var tasksWorkflowId = await debService.GetWorkflowIdAsync(moduleId, EntityTypes.Task, cancellationToken);

            var requirements = debService.GetRequirementsForStandardVersion(parameters.StandardVersionId);


            // Create one statement for each requirement

            // Create a random number of tasks per statement ranging from 0 to parameters.MaximumNumberOfTasksPerStatement

            // Save records


            return Results.Ok();
        }
    }
}
