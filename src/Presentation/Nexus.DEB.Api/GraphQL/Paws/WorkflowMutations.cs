using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL.Paws
{
    //[MutationType]
    //public static class WorkflowMutations
    //{
    //    [Authorize]
    //    public static async Task<TransitionApprovalResult> ApproveStepAsync(
    //        Guid entityId,
    //        int stepId,
    //        int triggerStatusId,
    //        int[] destinationActivityIDs,
    //        string? comments,
    //        Guid? onBehalfOfId,
    //        string? password,
    //        IDebService debService,
    //        IPawsService pawsService,
    //        IWorkflowSideEffectService workflowSideEffectService,
    //        IConfiguration configuration,
    //        CancellationToken cancellationToken)
    //    {
    //        Result result;

    //        var moduleIdString = configuration["Modules:DEB"] ?? throw new InvalidOperationException("Modules:DEB not configured in appsettings");

    //        if (!Guid.TryParse(moduleIdString, out var moduleId))
    //        {
    //            throw new InvalidOperationException("Modules:DEB must be a valid GUID");
    //        }

    //        var entity = await debService.GetEntityHeadAsync(entityId, cancellationToken);

    //        if (entity == null)
    //        {
    //            result = Result.Failure(new ValidationError
    //            {
    //                Field = "entity",
    //                Message = "Entity not found",
    //                Code = "ENTITY_NOT_FOUND"
    //            });

    //            throw BuildException(result);
    //        }

    //        var workflowId = await debService.GetWorkflowIdAsync(moduleId, entity.EntityTypeTitle, cancellationToken);

    //        if (workflowId == null)
    //        {
    //            result = Result.Failure(new ValidationError
    //            {
    //                Field = "workflowId",
    //                Message = "Workflow not found",
    //                Code = "WORKFLOW_NOT_FOUND"
    //            });

    //            throw BuildException(result);
    //        }

    //        var approved = await pawsService.ApproveStepAsync(
    //            workflowId.Value,
    //            entityId,
    //            stepId,
    //            triggerStatusId,
    //            destinationActivityIDs,
    //            comments,
    //            cancellationToken: cancellationToken);

    //        if (!approved)
    //        {
    //            result = Result.Failure(new ValidationError
    //            {
    //                Field = "statusId",
    //                Message = "Workflow step could not be approved",
    //                Code = "WORKFLOW_NOT_APPROVED"
    //            });

    //            throw BuildException(result);
    //        }

    //        result = await workflowSideEffectService.ExecuteSideEffectAsync(entityId, stepId, triggerStatusId, cancellationToken);

    //        if (!result.IsSuccess)
    //        {
    //            throw BuildException(result);
    //        }

    //        // TODO
    //        return new TransitionApprovalResult()
    //        {
    //            IsApproved = true
    //        };
    //    }

    //    private static GraphQLException BuildException(Result result)
    //    {
    //        var errors = result.Errors.Select(e =>
    //            ErrorBuilder.New()
    //                .SetMessage(e.Message)
    //                .SetCode(e.Code)
    //                .SetExtension("field", e.Field)
    //                .SetExtension("meta", e.Meta)
    //                .Build());

    //        return new GraphQLException(errors);
    //    }
    //}
}
