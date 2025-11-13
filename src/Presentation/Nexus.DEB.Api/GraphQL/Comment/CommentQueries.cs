using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class CommentQueries
    {
        [Authorize]
        public static async Task<ICollection<CommentDetail>> GetCommentsForEntityAsync(
            Guid entityId,
            IDebService debService,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken)
            => await debService.GetCommentsForEntityAsync(entityId, cancellationToken);

        public static async Task<int> GetCommentsCountForEntity(
            Guid entityId,
            IDebService debService,
            CancellationToken cancellationToken)
            => await debService.GetCommentsCountForEntityAsync(entityId, cancellationToken);
    }
}
