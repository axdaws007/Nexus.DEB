using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Models;

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
            CancellationToken cancellationToken)
        {
            var result = await sectionDomainService.MoveSectionAsync(sectionId, parentSectionId, ordinal, cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

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

            return result.Data;
        }

        [Authorize(Policy = DebHelper.Policies.CanEditStdVersion)]
        public static async Task<Section?> UpdateSectionAsync(
            Guid sectionId,
            string reference,
            string title,
            bool displayReference,
            bool displayTitle,
            ISectionDomainService sectionDomainService,
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

            return result.Data;
        }

        [Authorize(Policy = DebHelper.Policies.CanEditStdVersion)]
        [UseMutationConvention(Disable = true)]
        public static async Task<bool> DeleteSectionAsync(
            Guid sectionId,
            ISectionDomainService sectionDomainService,
            CancellationToken cancellationToken)
        {
            var result = await sectionDomainService.DeleteSectionAsync(sectionId, cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            return result.Data;
        }

        [Authorize(Policy = DebHelper.Policies.CanEditStdVersion)]
        public static async Task<SectionRequirementResponse?> UpdateRequirementsAssignedToSection(
            Guid sectionId,
            ICollection<Guid> idsToAdd,
            ICollection<Guid> idsToRemove,
            ISectionDomainService sectionDomainService, 
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

            return result.Data;
        }

        [Authorize(Policy = DebHelper.Policies.CanEditStdVersion)]
        public static async Task<bool> MoveRequirementsAssignedToSection(
            Guid requirementId,
            Guid oldSectionId,
            Guid newSectionId,
            int ordinal,
            ISectionDomainService sectionDomainService,
            CancellationToken cancellationToken)
        {
            var result = await sectionDomainService.MoveRequirementAssignedToSectionAsync(requirementId, oldSectionId, newSectionId, ordinal, cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            return true;
        }
    }
}
