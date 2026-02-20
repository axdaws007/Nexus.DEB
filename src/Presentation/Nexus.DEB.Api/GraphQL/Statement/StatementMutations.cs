using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Events;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [MutationType]
    public static class StatementMutations
    {
        [Authorize(Policy = DebHelper.Policies.CanCreateOrEditSoC)]
        public static async Task<StatementDetail?> CreateStatementAsync(
            Guid ownerId,
            string title,
            string statementText,
            DateOnly reviewDate,
            ICollection<RequirementScopes> requirementScopeCombinations,
            IStatementDomainService statementService,
            IDomainEventPublisher eventPublisher,
            CancellationToken cancellationToken = default)
        {
            var result = await statementService.CreateStatementAsync(
                ownerId,
                title,
                statementText,
                reviewDate,
                requirementScopeCombinations,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            var statementDetail = result.Data!;

            await eventPublisher.PublishAsync(new EntitySavedEvent
            {
                Entity = statementDetail,
                EntityType = statementDetail.EntityTypeTitle,
                EntityId = statementDetail.EntityId,
                SerialNumber = statementDetail.SerialNumber ?? string.Empty,
                IsNew = true,
            }, cancellationToken);

            return result.Data;
        }

        [Authorize(Policy = DebHelper.Policies.CanCreateOrEditSoC)]
        public static async Task<StatementDetail?> UpdateStatementAsync(
            Guid id,
            Guid ownerId,
            string title,
            string statementText,
            DateOnly reviewDate,
            ICollection<RequirementScopes> requirementScopeCombinations,
            IStatementDomainService statementService,
            IDomainEventPublisher eventPublisher,
            CancellationToken cancellationToken = default)
        {
            var result = await statementService.UpdateStatementAsync(
                id,
                ownerId,
                title,
                statementText,
                reviewDate,
                requirementScopeCombinations,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            var statementDetail = result.Data!;

            await eventPublisher.PublishAsync(new EntitySavedEvent
            {
                Entity = statementDetail,
                EntityType = statementDetail.EntityTypeTitle,
                EntityId = statementDetail.EntityId,
                SerialNumber = statementDetail.SerialNumber ?? string.Empty,
                IsNew = true,
            }, cancellationToken);

            return result.Data;
        }
    }
}
