using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Api.GraphQL
{
    [MutationType]
    public static class SectionMutations
    {
        [Authorize]
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
    }
}
