using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Api.Security;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [MutationType]
    public static class CommentMutations
    {
        [Authorize(Policy = DebHelper.Policies.CanAddComments)]
        public static async Task<CommentDetail?> CreateCommentAsync(
            Guid entityId,
            string text,
            ICommentDomainService commentDomainService,
            IResolverContext resolverContext,
            CancellationToken cancellationToken)
        {
            var debUser = new DebUser(resolverContext.GetUser());

            var result = await commentDomainService.CreateCommentAsync(entityId, text, debUser, cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            return result.Data;
        }

        [Authorize(Policy = DebHelper.Policies.CanDeleteComments)]
        [UseMutationConvention(Disable = true)]
        public static async Task<bool> DeleteCommentByIdAsync(
            [ID]long id,
            ICommentDomainService commentDomainService,
            IResolverContext resolverContext,
            CancellationToken cancellationToken)
        {
            var debUser = new DebUser(resolverContext.GetUser());

            var result = await commentDomainService.DeleteCommentByIdAsync(id, debUser, cancellationToken);

            if (!result.IsSuccess)
            {
                throw ExceptionHelper.BuildException(result);
            }

            return true;
        }
    }
}
