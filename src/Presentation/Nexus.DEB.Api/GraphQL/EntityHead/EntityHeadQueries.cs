using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL.EntityHead
{
    [QueryType]
    public static class EntityHeadQueries
    {
        [Authorize]
        public static async Task<EntityHeadDetail?> GetEntityHeadDetailsAsync(
            Guid id,
            IDebService debService,
            CancellationToken cancellationToken)
            => await debService.GetEntityHeadDetailAsync(id, cancellationToken);
    }
}
