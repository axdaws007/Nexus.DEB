using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Events;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Interfaces;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Api.GraphQL
{
    [MutationType]
    public static class SectionMutations
    {
        [Authorize(Policy = DebHelper.Policies.CanEditStdVersion)]
        public static async Task<bool> MoveSectionAsync(
            Guid sectionId,
            Guid? parentSectionId,
            int ordinal,
            ISectionDomainService sectionDomainService,
            IDomainEventPublisher eventPublisher,
            CancellationToken cancellationToken)
        {
            var result = await sectionDomainService.MoveSectionAsync(sectionId, parentSectionId, ordinal, cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            await eventPublisher.PublishAsync(new ChildEntitySavedEvent
            {
                ParentEntityType = EntityTypes.StandardVersion,
                ParentEntityId = result.Data.StandardVersionId,
                ChildEntityType = "Section",
                EventContext = $"Moved section {result?.Data?.Id}"
            }, cancellationToken);

            return result.IsSuccess;
        }

        [Authorize(Policy = DebHelper.Policies.CanEditStdVersion)]
        public static async Task<Section?> CreateSectionAsync(
            string reference,
            string title,
            bool displayReference,
            bool displayTitle,
            Guid? parentId,
            Guid standardVersionId,
            ISectionDomainService sectionDomainService,
            IDomainEventPublisher eventPublisher,
            CancellationToken cancellationToken)
        {
            var result = await sectionDomainService.CreateSectionAsync(
                reference,
                title,
                displayReference,
                displayTitle,
                parentId,
                standardVersionId,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            await eventPublisher.PublishAsync(new ChildEntitySavedEvent
            {
                ParentEntityType = EntityTypes.StandardVersion,
                ParentEntityId = standardVersionId,
                ChildEntityType = "Section",
                EventContext = $"Added new section {result?.Data?.Id}"
            }, cancellationToken);

            return result?.Data;
        }

        [Authorize(Policy = DebHelper.Policies.CanEditStdVersion)]
        public static async Task<Section?> UpdateSectionAsync(
            Guid sectionId,
            string reference,
            string title,
            bool displayReference,
            bool displayTitle,
            ISectionDomainService sectionDomainService,
            IDomainEventPublisher eventPublisher,
            CancellationToken cancellationToken)
        {
            var result = await sectionDomainService.UpdateSectionAsync(
                sectionId,
                reference,
                title,
                displayReference,
                displayTitle,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            await eventPublisher.PublishAsync(new ChildEntitySavedEvent
            {
                ParentEntityType = EntityTypes.StandardVersion,
                ParentEntityId = result.Data.StandardVersionId,
                ChildEntityType = "Section",
                EventContext = $"Updated section {result?.Data?.Id}"
            }, cancellationToken);

            return result.Data;
        }

        [Authorize(Policy = DebHelper.Policies.CanEditStdVersion)]
        [UseMutationConvention(Disable = true)]
        public static async Task<bool> DeleteSectionAsync(
            Guid sectionId,
            ISectionDomainService sectionDomainService,
            IDebService debService,
            IDomainEventPublisher eventPublisher,
            CancellationToken cancellationToken)
        {
            var result = await sectionDomainService.DeleteSectionAsync(sectionId, cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            var section = result.Data;

            if (section != null)
            {
                await eventPublisher.PublishAsync(new ChildEntitySavedEvent
                {
                    ParentEntityType = EntityTypes.StandardVersion,
                    ParentEntityId = section.StandardVersionId,
                    ChildEntityType = "Section",
                    EventContext = $"Deleted section {section.Id}"
                }, cancellationToken);
            }

            return result.IsSuccess;
        }

        [Authorize(Policy = DebHelper.Policies.CanEditStdVersion)]
        public static async Task<SectionRequirementResponse?> UpdateRequirementsAssignedToSection(
            Guid sectionId,
            ICollection<Guid> idsToAdd,
            ICollection<Guid> idsToRemove,
            ISectionDomainService sectionDomainService,
            IDebService debService,
            IDomainEventPublisher eventPublisher,
            CancellationToken cancellationToken,
            bool addAll = false,                       // Not used, but Stewart wanted it present for consistency
            bool removeAll = false                    // Not used, but Stewart wanted it present for consistency
            )
        {
            var result = await sectionDomainService.UpdateSectionRequirementsAsync(sectionId, idsToAdd, idsToRemove, cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            var section = await debService.GetSectionByIdAsync(sectionId, cancellationToken);

            await eventPublisher.PublishAsync(new ChildEntitySavedEvent
            {
                ParentEntityType = EntityTypes.StandardVersion,
                ParentEntityId = section.StandardVersionId,
                ChildEntityType = "Section",
                EventContext = $"Updated requirements assigned to section {section.Id}"
            }, cancellationToken);

            return result.Data;
        }

        [Authorize(Policy = DebHelper.Policies.CanEditStdVersion)]
        public static async Task<bool> MoveRequirementsAssignedToSection(
            Guid requirementId,
            Guid oldSectionId,
            Guid newSectionId,
            int ordinal,
            ISectionDomainService sectionDomainService,
            IDebService debService,
            IDomainEventPublisher eventPublisher,
            CancellationToken cancellationToken)
        {
            var result = await sectionDomainService.MoveRequirementAssignedToSectionAsync(requirementId, oldSectionId, newSectionId, ordinal, cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            var section = await debService.GetSectionByIdAsync(newSectionId, cancellationToken);

            await eventPublisher.PublishAsync(new ChildEntitySavedEvent
            {
                ParentEntityType = EntityTypes.StandardVersion,
                ParentEntityId = section.StandardVersionId,
                ChildEntityType = "Section",
                EventContext = $"Requirement ID {requirementId} moved from {oldSectionId} to {newSectionId}"
            }, cancellationToken);

            return true;
        }
    }
}
