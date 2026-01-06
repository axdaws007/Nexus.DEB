using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Nexus.DEB.Application.Common.Extensions;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using System.Reflection;
using System.Threading;
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
            IPawsService pawsService,
            IAuditService auditService) : base(cisService, cbacService, applicationSettingsService, currentUserService, dateTimeProvider, debService, pawsService, auditService, EntityTypes.Task)
        {
        }

        public async Task<Result<TaskDetail>> CreateTaskAsync(Guid statementId, Guid taskOwnerId, short taskTypeId, int activityId, DateTime? dueDate, string title, string? description, CancellationToken cancellationToken = default)
        {
            await ValidateFieldsAsync(null, statementId, taskOwnerId, taskTypeId, activityId, dueDate, title, description);

            if (ValidationErrors.Count > 0)
            {
                return Result<TaskDetail>.Failure(ValidationErrors);
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
                    TaskTypeId = taskTypeId,
                    EntityTypeTitle = EntityTypes.Task,
                    SerialNumber = await DebService.GenerateSerialNumberAsync(this.ModuleId, this.InstanceId, EntityTypes.Task)
                };

                task = await this.DebService.CreateTaskAsync(task, cancellationToken);

                await this.PawsService.CreateWorkflowInstanceAsync(this.WorkflowId.Value, task.EntityId, activityId, taskOwnerId, cancellationToken);

                var taskDetail = await this.DebService.GetTaskDetailByIdAsync(task.EntityId, cancellationToken);

                await this.AuditService.EntitySaved(
                    taskDetail.EntityId,
                    EntityTypes.Task,
                    $"Task {taskDetail.SerialNumber} created.",
                    await this.CurrentUserService.GetUserDetailsAsync(),
                    taskDetail.ToAuditData());

                return Result<TaskDetail>.Success(taskDetail);
            }
            catch (Exception ex)
            {
                return Result<TaskDetail>.Failure($"An error occurred creating the Task: {ex.Message}");
            }
        }

        public async Task<Result<TaskDetail>> UpdateTaskAsync(Guid id, Guid statementId, Guid taskOwnerId, short taskTypeId, int activityId, DateTime? dueDate, string title, string? description, CancellationToken cancellationToken = default)
        {
            var task = await DebService.GetTaskByIdAsync(id);

            if (task == null)
            {
                return Result<TaskDetail>.Failure(new ValidationError()
                {
                    Code = "INVALID_TASK_ID",
                    Field = nameof(id),
                    Message = "Task does not exist"
                });
            }

            await ValidateFieldsAsync(null, statementId, taskOwnerId, taskTypeId, activityId, dueDate, title, description);

            if (ValidationErrors.Count > 0)
            {
                return Result<TaskDetail>.Failure(ValidationErrors);
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
				if (activityId != workflowStatus.ActivityId)
				{
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

					await AddChangeRecordItemForPAWSChange(task.EntityId, workflowStatus.ActivityTitle, activityId, cancellationToken);
				}

                var taskDetail = await this.DebService.GetTaskDetailByIdAsync(task.EntityId, cancellationToken);

                await this.AuditService.EntitySaved(
                    taskDetail.EntityId,
                    EntityTypes.Task,
                    $"Task {taskDetail.SerialNumber} updated.",
                    await this.CurrentUserService.GetUserDetailsAsync(),
                    taskDetail.ToAuditData());

                return Result<TaskDetail>.Success(taskDetail);
            }
            catch (Exception ex)
            {
                return Result<TaskDetail>.Failure($"An error occurred updating the Task: {ex.Message}");
            }

        }

        private async Task AddChangeRecordItemForPAWSChange(Guid entityId, string oldActivity, int newActivityId, CancellationToken cancellationToken)
        {
			var moduleId = ApplicationSettingsService.GetModuleId("DEB");
			var workflowId = await DebService.GetWorkflowIdAsync(moduleId, "Task", cancellationToken);
			var activities = await PawsService.GetActivitiesForWorkflowAsync(workflowId.Value, false, cancellationToken);
			var newActivity = activities.FirstOrDefault(f => f.ActivityID == newActivityId)?.Title.ToString();

			await DebService.AddChangeRecordItem(entityId, "PendingActivity", "State", oldActivity, newActivity, cancellationToken);
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
