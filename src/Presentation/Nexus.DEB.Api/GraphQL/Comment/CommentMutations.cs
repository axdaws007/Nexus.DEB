using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [MutationType]
    public static class CommentMutations
    {
        public static async Task<CommentDetail?> CreateCommentAsync(
            Guid entityId,
            string text,
            IDebService debService,
            ICisService cisService,
            ICurrentUserService currentUserService,
            IDateTimeProvider dateTimeProvider,
            CancellationToken cancellationToken)
        {
            var cisInfo = await cisService.GetUserDetailsAsync(currentUserService.UserId, currentUserService.PostId);

            if (cisInfo == null)
            {
                throw new InvalidDataException("Unable to retrieve the user and post information");
            }

            var comment = new Comment()
            {
                CommentTypeId = null,
                CreatedByPostId = cisInfo?.PostId,
                CreatedByPostTitle = cisInfo?.PostTitle,
                CreatedByUserId = cisInfo?.UserId,
                CreatedByUserName = cisInfo?.UserName,
                CreatedDate = dateTimeProvider.Now,
                EntityId = entityId,
                Text = text
            };

            return await debService.CreateCommentAsync(comment, cancellationToken);
        }
    }
}
