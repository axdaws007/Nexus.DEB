using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;

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

            if (roleIds != null && roleIds.Count > 0)
            {
                var postIds = await cbacService.GetRolePostIdsAsync(roleIds);

                return posts.Where(x => postIds.Contains(x.ID)).ToList();
            }

            if (posts != null && posts.Count > 0 && !string.IsNullOrEmpty(searchText))
                return posts.Where(x => x.PostTitle.ToLower().Contains(searchText.ToLower())).OrderBy(x => x.PostTitle).ToList();

            return posts ?? new List<PostDetails>();
        }
    }
}
