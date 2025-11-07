using Microsoft.AspNetCore.Mvc;
using Nexus.DEB.Api.Restful.Models;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Api.Restful
{
    public static class TestDataEndpoints
    {
        public static void MapTestDataEndpoints(this WebApplication app)
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
