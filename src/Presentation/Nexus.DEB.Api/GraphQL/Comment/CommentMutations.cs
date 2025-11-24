using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Api.Security;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Exceptions;
using Nexus.DEB.Domain.Models;
using System.Security.Claims;

namespace Nexus.DEB.Api.GraphQL
{
    [MutationType]
    public static class CommentMutations
    {
//        [Authorize(Policy = DebHelper.Policies.CanAddComments)]
        [Authorize]
        [Error<EntityNotFoundException>]
        [Error<CapabilityException>]
        public static async Task<CommentDetail?> CreateCommentAsync(
            Guid entityId,
            string text,
            IDebService debService,
            ICisService cisService,
            ICurrentUserService currentUserService,
            IDateTimeProvider dateTimeProvider,
            IResolverContext resolverContext,
            CancellationToken cancellationToken)
        {
            var debUser = new DebUser(resolverContext.GetUser());

            var comment = new Comment()
            {
                CommentTypeId = null,
                CreatedByPostId = debUser.PostId,
                CreatedByPostTitle = debUser.PostTitle,
                CreatedByUserId = debUser.UserId,
                CreatedByUserName = debUser.UserName,
                CreatedDate = dateTimeProvider.Now,
                EntityId = entityId,
                Text = text
            };

            return await debService.CreateCommentAsync(comment, cancellationToken);
        }

        [Authorize]
        [UseMutationConvention(Disable = true)]
        [Error<EntityNotFoundException>]
        [Error<UnauthorisedException>]
        public static async Task<bool> DeleteCommentByIdAsync(
            [ID]long id,
            IDebService debService,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken)
        {
            var comment = await debService.GetCommentByIdAsync(id, cancellationToken);

            if (comment == null)
                throw new EntityNotFoundException(nameof(Comment), id);

            // TODO : add code to check whether the user can delete based upon their capabilities.
            return await debService.DeleteCommentByIdAsync(id, cancellationToken);
        }
    }
}
