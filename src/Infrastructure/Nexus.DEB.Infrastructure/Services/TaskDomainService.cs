using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models.Common;
using Task = System.Threading.Tasks.Task;

namespace Nexus.DEB.Infrastructure.Services
{
    public class TaskDomainService : DomainServiceBase, ITaskDomainService
    {
        public TaskDomainService(
            ICisService cisService,
            ICbacService cbacService,
            IApplicationSettingsService applicationSettingsService,
            ICurrentUserService currentUserService,
            IDateTimeProvider dateTimeProvider,
            IDebService debService,
            IPawsService pawsService) : base(cisService, cbacService, applicationSettingsService, currentUserService, dateTimeProvider, debService, pawsService, EntityTypes.Task)
        {
        }

        public async Task<Result<Domain.Models.Task>> CreateTaskAsync(Guid statementId, Guid taskOwnerId, short taskTypeId, int activityId, DateTime? dueDate, string title, string? description, CancellationToken cancellationToken = default)
        {
            await ValidateFieldsAsync(null, statementId, taskOwnerId, taskTypeId, activityId, dueDate, title, description);

            if (ValidationErrors.Count > 0)
            {
                return Result<Domain.Models.Task>.Failure(ValidationErrors);
            }

            try
            {
                var task = new Domain.Models.Task()
                {
                    StatementId = statementId,
                    OwnedById = taskOwnerId,
                    Title = title,
                    Description = description,
                    DueDate = dueDate,
                    TaskTypeId = taskTypeId
                };

                task = await this.DebService.CreateTaskAsync(task, cancellationToken);

                await this.PawsService.CreateWorkflowInstanceAsync(this.WorkflowId.Value, task.EntityId, activityId, taskOwnerId, cancellationToken);
                
                return Result<Domain.Models.Task>.Success(task);
            }
            catch (Exception ex)
            {
                return Result<Domain.Models.Task>.Failure($"An error occurred creating the Task: {ex.Message}");
            }
        }

        public async Task<Result<Domain.Models.Task>> UpdateTaskAsync(Guid id, Guid statementId, Guid taskOwnerId, short taskTypeId, int activityId, DateTime? dueDate, string title, string? description, CancellationToken cancellationToken = default)
        {
            var task = await DebService.GetTaskByIdAsync(id);

            if (task == null)
            {
                return Result<Domain.Models.Task>.Failure(new ValidationError()
                {
                    Code = "INVALID_TASK_ID",
                    Field = nameof(id),
                    Message = "Task does not exist"
                });
            }

            await ValidateFieldsAsync(null, statementId, taskOwnerId, taskTypeId, activityId, dueDate, title, description);

            if (ValidationErrors.Count > 0)
            {
                return Result<Domain.Models.Task>.Failure(ValidationErrors);
            }

            task.OwnedById = taskOwnerId;
            task.TaskTypeId = taskTypeId;
            task.DueDate = dueDate;
            task.Title = title;
            task.Description = description;

            try
            {
                await this.DebService.UpdateTaskAsync(task, cancellationToken);

                var workflowStatus = await DebService.GetCurrentWorkflowStatusForEntityAsync(task.EntityId, cancellationToken);

                await PawsService.ApproveStepAsync(
                    this.WorkflowId.Value,
                    task.EntityId,
                    workflowStatus.StepId,
                    activityId,
                    [activityId],
                    null,
                    null,
                    null,
                    [taskOwnerId],
                    cancellationToken);

                return Result<Domain.Models.Task>.Success(task);
            }
            catch (Exception ex)
            {
                return Result<Domain.Models.Task>.Failure($"An error occurred updating the Task: {ex.Message}");
            }

        }

        private async Task ValidateFieldsAsync(
            Domain.Models.Task? task,
            Guid statementId, 
            Guid taskOwnerId, 
            short taskTypeId, 
            int statusId, 
            DateTime? dueDate, 
            string title, 
            string? description)
        {
            await ValidateStatement(statementId);

            await ValidateOwnerAsync(taskOwnerId);

            ValidateTitle(title);

            ValidateTaskType(taskTypeId);
        }

        private void ValidateTaskType(short taskTypeId)
        {
            var taskType = DebService.GetTaskTypes().FirstOrDefault(x => x.Id == taskTypeId);

            if (taskType == null)
            {
                ValidationErrors.Add(
                    new ValidationError()
                    {
                        Code = "INVALID_TASK_TYPE",
                        Field = nameof(taskTypeId),
                        Message = "An invalid task type ID was provided."
                    });
            }
        }

        private async Task ValidateStatement(Guid statementId)
        {
            var statement = await DebService.GetStatementByIdAsync(statementId);

            if (statement == null || statement.IsRemoved)
            {
                ValidationErrors.Add(
                    new ValidationError()
                    {
                        Code = "INVALID_STATEMENT",
                        Field = nameof(statementId),
                        Message = "An invalid statement ID was provided."
                    });
            }
        }
    }
}
