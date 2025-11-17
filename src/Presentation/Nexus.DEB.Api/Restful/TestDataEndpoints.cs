using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Api.Restful.Models;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using Task = System.Threading.Tasks.Task;

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
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IConfiguration configuration,
            [FromServices] ILogger<Program> logger,
            [FromServices] IHttpContextAccessor httpContextAccessor, // For current user
            CancellationToken cancellationToken)
        {
            // Validation
            if (parameters == null)
                return Results.BadRequest("Parameters are required");

            var moduleIdString = configuration["Modules:DEB"]
                ?? throw new InvalidOperationException("Modules:DEB not configured in appsettings");

            if (!Guid.TryParse(moduleIdString, out var moduleId))
                throw new InvalidOperationException("Modules:DEB must be a valid GUID");

            // Get current user ID (adjust based on your auth setup)
            var currentPostId = currentUserService.PostId;

            // Get workflow IDs
            var statementWorkflowId = await debService.GetWorkflowIdAsync(moduleId, EntityTypes.SoC, cancellationToken);
            var tasksWorkflowId = await debService.GetWorkflowIdAsync(moduleId, EntityTypes.Task, cancellationToken);

            // Get requirements and task types
            var requirements = await debService.GetRequirementsForStandardVersion(parameters.StandardVersionId)
                .ToListAsync(cancellationToken);

            var taskTypes = await debService.GetTaskTypes()
                .Where(tt => tt.IsEnabled)
                .ToListAsync(cancellationToken);

            if (!requirements.Any())
                return Results.BadRequest("No requirements found for the specified StandardVersion");

            if (!taskTypes.Any())
                return Results.BadRequest("No enabled task types found in the system");

            // Get scope (you'll need to determine which scope to use - perhaps from parameters?)
            var scopes = await debService.GetScopesLookupAsync(cancellationToken);

            // Determine possible owner IDs
            var possibleOwnerIds = parameters.PossiblePostIds ?? new List<Guid> { currentPostId };

            // Initialize Bogus
            var random = new Randomizer();

            var f = new Faker();
            var createdDate = f.Date.Past(yearsToGoBack: 1, refDate: DateTime.UtcNow.AddMonths(-1));

            // Create Faker for Statements
            var statementFaker = new Faker<Statement>()
                .RuleFor(s => s.ModuleId, moduleId)
                .RuleFor(s => s.Title, f => f.Lorem.Sentence())
                .RuleFor(s => s.Description, f => f.Lorem.Paragraph())
                .RuleFor(s => s.StatementText, f => f.Lorem.Paragraphs(2, 4))
                .RuleFor(s => s.SerialNumber, f => $"SoC-" + (f.IndexFaker + 1).ToString("D3"))
                .RuleFor(s => s.ReviewDate, f => f.Date.Future(1))
                .RuleFor(s => s.ScopeID, f => f.PickRandom(scopes).Id)
                .RuleFor(s => s.OwnedById, f => f.PickRandom(possibleOwnerIds))
                .RuleFor(s => s.OwnedByGroupId, (Guid?)null)
                .RuleFor(s => s.CreatedById, f => f.PickRandom(possibleOwnerIds))
                .RuleFor(s => s.LastModifiedById, f => f.PickRandom(possibleOwnerIds))
                .RuleFor(s => s.CreatedDate, createdDate)
                .RuleFor(s => s.LastModifiedDate, f.Date.Between(createdDate, DateTime.UtcNow))
                .RuleFor(s => s.IsRemoved, false)
                .RuleFor(s => s.IsArchived, false)
                .RuleFor(s => s.EntityTypeTitle, EntityTypes.SoC);

            // Create Faker for Tasks
            var taskFaker = new Faker<Domain.Models.Task>()
                .RuleFor(t => t.ModuleId, moduleId)
                .RuleFor(t => t.Title, f => f.Lorem.Sentence())
                .RuleFor(t => t.Description, f => f.Lorem.Paragraph())
                .RuleFor(t => t.SerialNumber, f => $"T-" + (f.IndexFaker + 1).ToString("D5"))
                .RuleFor(t => t.TaskTypeId, f => f.PickRandom(taskTypes).Id)
                .RuleFor(t => t.DueDate, f => f.Date.Between(DateTime.UtcNow.AddDays(7), DateTime.UtcNow.AddDays(90)))
                .RuleFor(t => t.OwnedById, f => f.PickRandom(possibleOwnerIds))
                .RuleFor(t => t.OwnedByGroupId, (Guid?)null)
                .RuleFor(t => t.CreatedById, f => f.PickRandom(possibleOwnerIds))
                .RuleFor(t => t.LastModifiedById, f => f.PickRandom(possibleOwnerIds))
                .RuleFor(t => t.CreatedDate, DateTime.UtcNow)
                .RuleFor(t => t.LastModifiedDate, DateTime.UtcNow)
                .RuleFor(t => t.IsRemoved, false)
                .RuleFor(t => t.IsArchived, false)
                .RuleFor(t => t.EntityTypeTitle, EntityTypes.Task);

            var statementsToCreate = new List<Statement>();
            var tasksToCreate = new List<Domain.Models.Task>();

            // Create one statement for each requirement
            foreach (var requirement in requirements)
            {
                var statement = statementFaker.Generate();

                statement.Requirements = [requirement];

                statementsToCreate.Add(statement);

                await CreateRandomWorkflowSteps(statementWorkflowId.Value, statement.EntityId, pawsService, f, cancellationToken);

                // Create a random number of tasks per statement (0 to MaximumNumberOfTasksPerStatement)
                var numberOfTasks = random.Number(0, parameters.MaximumNumberOfTasksPerStatement);

                for (int i = 0; i < numberOfTasks; i++)
                {
                    var task = taskFaker.Generate();
                    task.StatementId = statement.EntityId;
                    tasksToCreate.Add(task);

                    await CreateRandomWorkflowSteps(tasksWorkflowId.Value, task.EntityId, pawsService, f, cancellationToken);
                }
            }

            logger.LogInformation(
                "Generating {StatementCount} statements and {TaskCount} tasks for StandardVersion {StandardVersionId}",
                statementsToCreate.Count, tasksToCreate.Count, parameters.StandardVersionId);

            // Save to database
            try
            {
                await debService.SaveStatementsAndTasks(statementsToCreate, tasksToCreate, cancellationToken);

                logger.LogInformation(
                    "Successfully created {StatementCount} statements and {TaskCount} tasks",
                    statementsToCreate.Count, tasksToCreate.Count);

                return Results.Ok(new
                {
                    StatementsCreated = statementsToCreate.Count,
                    TasksCreated = tasksToCreate.Count,
                    Message = "Test data generated successfully"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error generating test data");
                return Results.Problem("An error occurred while generating test data");
            }
        }

        private static async Task CreateRandomWorkflowSteps(
            Guid workflowId,
            Guid entityId,
            IPawsService pawsService,
            Faker f,
            CancellationToken cancellationToken)
        {
            await pawsService.CreateWorkflowInstanceAsync(workflowId, entityId);

            (bool generateNextStep, double trueProbability) = GenerateNextStep(0.70);

            while (generateNextStep)
            {
                var pendingSteps = await pawsService.GetPendingActivitiesAsync(entityId, workflowId, cancellationToken);
                var step = pendingSteps?.FirstOrDefault();
                if (step == null)
                    break;

                if (step.AvailableTriggerStates == null || step.AvailableTriggerStates.Count == 0)
                    break;

                int numberOfPossibilities = step.AvailableTriggerStates.Count;
                var availableStates = step.AvailableTriggerStates.ToArray();

                var choice = Random.Shared.Next(numberOfPossibilities);
                var triggerStatus = availableStates[choice];
                if (triggerStatus == null)
                    break;

                var destinationSteps = await pawsService.GetDestinationActivitiesAsync(
                    step.StepID,
                    triggerStatus.ActivityStatusID,
                    cancellationToken);

                if (destinationSteps == null)
                    break;

                var comments = (string?)null;

                var nextStep = destinationSteps?.TargetActivities?.FirstOrDefault();
                if (nextStep == null)
                    break;

                if (nextStep.IsCommentRequired)
                {
                    comments = f.Lorem.Sentence();
                }

                var approved = await pawsService.ApproveStepAsync(
                    workflowId,
                    entityId,
                    step.StepID,
                    triggerStatus.ActivityStatusID,
                    new[] { nextStep.DestinationActivityID },
                    comments);

                if (!approved)
                    break;

                // Randomly decide whether to continue
                (generateNextStep, trueProbability) = GenerateNextStep(trueProbability);
            }

            return;
        }

        private static (bool, double) GenerateNextStep(double trueProbability)
        {
            // Generate a random value
            bool result = Random.Shared.NextDouble() < trueProbability;

            // Adjust weights for next time
            trueProbability = Math.Max(0, trueProbability - 0.05);

            return (result, trueProbability);
        }
    }
}
