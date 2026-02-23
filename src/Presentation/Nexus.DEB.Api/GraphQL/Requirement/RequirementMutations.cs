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
    public static class RequirementMutations
    {
        [Authorize(Policy = DebHelper.Policies.CanCreateOrEditRequirement)]
        public static async Task<RequirementDetail?> CreateRequirementAsync(
            Guid ownerId,
            string serialNumber,
            string title,
            string description,
            DateOnly effectiveStartDate,
            DateOnly effectiveEndDate,
            bool displayTitle,
            bool displayReference,
            short? requirementCategoryId,
            short? requirementTypeId,
            int? complianceWeighting,
            IRequirementDomainService requirementService,
            IDomainEventPublisher eventPublisher,
            CancellationToken cancellationToken = default)
        {
            var result = await requirementService.CreateRequirementAsync(
                ownerId,
                serialNumber,
                title,
                description,
                effectiveStartDate,
                effectiveEndDate,
                displayTitle,
                displayReference,
                requirementCategoryId,
                requirementTypeId,
                complianceWeighting,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            var requirementDetail = result.Data!;

            await eventPublisher.PublishAsync(new EntitySavedEvent
            {
                Entity = requirementDetail,
                EntityType = requirementDetail.EntityTypeTitle,
                EntityId = requirementDetail.EntityId,
                SerialNumber = requirementDetail.SerialNumber ?? string.Empty,
                IsNew = true,
            }, cancellationToken);

            return result.Data;
        }

        [Authorize(Policy = DebHelper.Policies.CanCreateOrEditRequirement)]
        public static async Task<RequirementDetail?> UpdateRequirementAsync(
            Guid id,
            Guid ownerId,
            string serialNumber,
            string title,
            string description,
            DateOnly effectiveStartDate,
            DateOnly effectiveEndDate,
            bool displayTitle,
            bool displayReference,
            short? requirementCategoryId,
            short? requirementTypeId,
            int? complianceWeighting,
            IRequirementDomainService requirementService,
            IDomainEventPublisher eventPublisher,
            CancellationToken cancellationToken = default)
        {
            var result = await requirementService.UpdateRequirementAsync(
                id,
                ownerId,
                serialNumber,
                title,
                description,
                effectiveStartDate,
                effectiveEndDate,
                displayTitle,
                displayReference,
                requirementCategoryId,
                requirementTypeId,
                complianceWeighting,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            var requirementDetail = result.Data!;

            await eventPublisher.PublishAsync(new EntitySavedEvent
            {
                Entity = requirementDetail,
                EntityType = requirementDetail.EntityTypeTitle,
                EntityId = requirementDetail.EntityId,
                SerialNumber = requirementDetail.SerialNumber ?? string.Empty,
                IsNew = false,
            }, cancellationToken);

            return result.Data;
        }
    }
}
