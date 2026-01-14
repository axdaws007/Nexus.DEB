using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL.Cis
{
    [QueryType]
    public static class CisAndCbacQueries
    {
        public static async Task<ICollection<PostDetails>> GetPosts(
            string? searchText,
            ICollection<Guid>? roleIds,
            ICisService cisService,
            ICbacService cbacService,
            CancellationToken cancellationToken)
        {
            var posts = await cisService.GetAllPosts();

            if (posts != null && posts.Count > 0)
            {
                var query = posts.AsQueryable();

                if (roleIds != null && roleIds.Count > 0)
                {
                    var postIds = await cbacService.GetRolePostIdsAsync(roleIds);

                    if (postIds != null)
                    {
                        query = query.Where(x => postIds.Contains(x.ID));
                    }
                }

                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(x => x.PostTitle.Contains(searchText, StringComparison.CurrentCultureIgnoreCase));
                }

                posts = query.OrderBy(x => x.PostTitle).ToList();
            }

            return posts ?? [];
        }

        public static async Task<ICollection<CisGroup>> GetGroups(
            string? searchText,
            ICisService cisService,
            ICbacService cbacService,
            CancellationToken cancellationToken)
        {
            var groups = await cisService.GetAllGroups();

            if (groups != null && groups.Count > 0)
            {
                var query = groups.AsQueryable();

                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(x => x.Name.Contains(searchText, StringComparison.CurrentCultureIgnoreCase));
                }

                groups = query.OrderBy(x => x.Name).ToList();
            }

            return groups ?? [];
        }

        [Authorize]
        [UseOffsetPaging]
        [UseSorting]
        public static IQueryable<UserAndPost> GetPostsWithUsers(
            IDebService debService,
            string? searchText, 
            bool includeDeletedUsers = false, 
            bool includeDeletedPosts = false)
            => debService.GetPostsWithUsers(searchText, includeDeletedUsers, includeDeletedPosts);
    }
}
