using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Api.Restful.Maps;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Filters;
using System.Formats.Asn1;
using System.Globalization;
using System.Text;

namespace Nexus.DEB.Api.Restful
{
    public static class ExportEndpoints
    {
        public static void MapExportEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/export")
                .WithTags("Export")
                .WithOpenApi();

            group.MapPost("/standard-versions-csv", ExportStandardVersionsAsCsv)
                .RequireAuthorization()
                .WithName("ExportStandardVersionsAsCsv")
                .WithSummary("Export standard versions as CSV file")
                .Produces<FileResult>(StatusCodes.Status200OK, contentType: "text/csv")
                .Produces(StatusCodes.Status401Unauthorized);
        }

        private static async Task<IResult> ExportStandardVersionsAsCsv(
            [FromBody] StandardVersionSummaryFilters? filters,
            [FromServices] IDebService debService,
            [FromServices] ILogger<Program> logger,
            CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Export request received for StandardVersions with filters: {@Filters}", filters);

                // Use the same method as your GraphQL query
                var data = await debService.GetStandardVersionsForExportOrGrid(filters)
                    .ToListAsync(cancellationToken);

                logger.LogInformation("Retrieved {Count} standard versions for export", data.Count);

                // Generate CSV using CsvHelper
                using var memoryStream = new MemoryStream();
                using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                });

                // Optional: Register a class map if you want to customize headers
                csv.Context.RegisterClassMap<StandardVersionSummaryMap>();

                await csv.WriteRecordsAsync(data, cancellationToken);
                await writer.FlushAsync();

                var csvBytes = memoryStream.ToArray();
                var fileName = $"standard-versions-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

                logger.LogInformation("Returning CSV file: {FileName} ({Size} bytes)", fileName, csvBytes.Length);

                return Results.File(
                    csvBytes,
                    contentType: "text/csv",
                    fileDownloadName: fileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during StandardVersions CSV export");
                return Results.Problem(
                    title: "Export Failed",
                    detail: "An error occurred while generating the CSV export. Please try again.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
