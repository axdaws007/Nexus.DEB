using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class SectionQueries
    {
        [Authorize]
        public static async Task<IReadOnlyList<Section>> GetSectionsForStandardVersionAsync(Guid id, IDebService debService, CancellationToken cancellationToken)
            => await debService.GetSectionsForStandardVersionAsync(id, cancellationToken);

        [Authorize]
        public static async Task<Section?> GetSectionByIdAsync(Guid id, IDebService debService, CancellationToken cancellationToken)
            => await debService.GetSectionByIdAsync(id, cancellationToken);

        [Authorize]
        public static async Task<IReadOnlyList<Guid>> GetRequirementIdsForSection(Guid sectionId, IDebService debService, CancellationToken cancellationToken)
            => await debService.GetRequirementIdsForSectionAsync(sectionId, cancellationToken);
    }
}
