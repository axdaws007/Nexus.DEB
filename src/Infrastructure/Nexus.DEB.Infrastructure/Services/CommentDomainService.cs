using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Extensions;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Infrastructure.Services
{
    public class CommentDomainService : DomainServiceBase, ICommentDomainService
    {
        public CommentDomainService(
            ICisService cisService,
            ICbacService cbacService,
            IApplicationSettingsService applicationSettingsService,
            ICurrentUserService currentUserService,
            IDateTimeProvider dateTimeProvider,
            IDebService debService,
            IPawsService pawsService,
            IAuditService auditService,
            ILogger<CommentDomainService> logger) : base(cisService, cbacService, applicationSettingsService, currentUserService, dateTimeProvider, debService, pawsService, auditService, logger, string.Empty)
        {
        }

        public async Task<Result<CommentDetail>> CreateCommentAsync(
            Guid entityId,
            string text,
            IDebUser debUser,
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await this.DebService.GetEntityHeadAsync(entityId, cancellationToken);

                if (entity == null)
                {
                    return Result<CommentDetail>.Failure(new ValidationError()
                    {
                        Code = "INVALID_ENTITY_ID",
                        Field = nameof(entityId),
                        Message = $"Entity {entityId} does not exist"
                    });
                }

                var requiredCapability = entity.EntityTypeTitle switch
                {
                    EntityTypes.StandardVersion => DebHelper.Capabilites.CanCreateStdVersionComments,
                    EntityTypes.Requirement => DebHelper.Capabilites.CanCreateRequirementComments,
                    EntityTypes.SoC => DebHelper.Capabilites.CanCreateSoCComments,
                    EntityTypes.Task => DebHelper.Capabilites.CanCreateTaskComments,
                    EntityTypes.Scope => DebHelper.Capabilites.CanCreateScopeComments,
                    _ => null
                };

                if (requiredCapability == null || !debUser.Capabilities.Contains(requiredCapability))
                {
                    return Result<CommentDetail>.Failure(new ValidationError()
                    {
                        Code = "CAPABILITY_ERROR",
                        Field = "Capabilities",
                        Message = $"This user does not have an appropriate capability."
                    });
                }

                var comment = new Comment()
                {
                    CommentTypeId = null,
                    CreatedByPostId = debUser.PostId,
                    CreatedByPostTitle = debUser.PostTitle,
                    CreatedByUserId = debUser.UserId,
                    CreatedByUserName = debUser.UserName,
                    CreatedDate = this.DateTimeProvider.Now,
                    EntityId = entityId,
                    Text = text
                };

                var commentDetail = await this.DebService.CreateCommentAsync(comment, cancellationToken);

                if (commentDetail == null)
                {
                    return Result<CommentDetail>.Failure("Comment was not created.");
                }

                await this.AuditService.EntitySaved(
                    comment.Id,
                    nameof(Comment),
                    $"Comment {comment.Id} created.",
                    await this.CurrentUserService.GetUserDetailsAsync(),
                    comment.ToAuditData());

                return Result<CommentDetail>.Success(commentDetail);
            }
            catch (Exception ex)
            {
                return Result<CommentDetail>.Failure(ex.Message);
            }
        }

        public async Task<Result> DeleteCommentByIdAsync(
            long id,
            IDebUser debUser,
            CancellationToken cancellationToken)
        {
            var comment = await this.DebService.GetCommentByIdAsync(id, cancellationToken);

            if (comment == null)
            {
                return Result.Failure(new ValidationError()
                {
                    Code = "INVALID_ENTITY_ID",
                    Field = nameof(id),
                    Message = $"Comment {id} does not exist"
                });
            }

            var entity = await this.DebService.GetEntityHeadAsync(comment.EntityId, cancellationToken);

            if (entity == null)
            {
                return Result.Failure(new ValidationError()
                {
                    Code = "INVALID_ENTITY_ID",
                    Field = nameof(comment.EntityId),
                    Message = $"Entity {comment.EntityId} does not exist"
                });
            }

            var requiredCapabilityDeleteAny = entity.EntityTypeTitle switch
            {
                EntityTypes.StandardVersion => DebHelper.Capabilites.CanDeleteAllStdVersionComments,
                EntityTypes.Requirement => DebHelper.Capabilites.CanDeleteAllRequirementComments,
                EntityTypes.SoC => DebHelper.Capabilites.CanDeleteAllSoCComments,
                EntityTypes.Task => DebHelper.Capabilites.CanDeleteAllTaskComments,
                EntityTypes.Scope => DebHelper.Capabilites.CanDeleteAllScopeComments,
                _ => null
            };

            var requiredCapabilityDeleteOwned = entity.EntityTypeTitle switch
            {
                EntityTypes.StandardVersion => DebHelper.Capabilites.CanDeleteOwnedStdVersionComments,
                EntityTypes.Requirement => DebHelper.Capabilites.CanDeleteOwnedRequirementComments,
                EntityTypes.SoC => DebHelper.Capabilites.CanDeleteOwnedSoCComments,
                EntityTypes.Task => DebHelper.Capabilites.CanDeleteOwnedTaskComments,
                EntityTypes.Scope => DebHelper.Capabilites.CanDeleteOwnedScopeComments,
                _ => null
            };

            var canDeleteAnyComment = requiredCapabilityDeleteAny != null && debUser.Capabilities.Contains(requiredCapabilityDeleteAny);
            var canDeleteOwnComment = requiredCapabilityDeleteOwned != null && debUser.Capabilities.Contains(requiredCapabilityDeleteOwned) && comment.CreatedByPostId == debUser.PostId;

            if (!canDeleteAnyComment && !canDeleteOwnComment)
            {
                return Result.Failure(new ValidationError()
                {
                    Code = "CAPABILITY_ERROR",
                    Field = "Capabilities",
                    Message = $"This user does not have an appropriate capability."
                });
            }

            try
            {
                var isDeleted = await this.DebService.DeleteCommentByIdAsync(id, cancellationToken);

                if (!isDeleted)
                {
                    return Result.Failure("The Comment could not be deleted.");
                }

                await this.AuditService.EntityDeleted(
                    comment.Id,
                    nameof(Comment),
                    $"Comment {comment.Id} deleted. Data field contains the Comment prior to deletion.",
                    await this.CurrentUserService.GetUserDetailsAsync(),
                    comment.ToAuditData());

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"An error occurred deleting the Comment: {ex.Message}");
            }
        }
    }
}
