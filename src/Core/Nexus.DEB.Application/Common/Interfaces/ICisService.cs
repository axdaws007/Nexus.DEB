using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ICisService
    {
        Task<CisUser?> ValidateCredentialsAsync(string username, string password);

        Task<bool> ValidatePostAsync(Guid userId, Guid postId);

        Task<UserDetails?> GetUserDetailsAsync(Guid userId, Guid postId);

        Task<IReadOnlyDictionary<Guid, string?>> GetNamesByIdsAsync(
            List<Guid> ids,
            CancellationToken cancellationToken = default);

        Task<ICollection<PostDetails>?> GetPostsBySearchTextAsync(string searchText, CancellationToken cancellation = default);
        Task<ICollection<PostDetails>?> GetAllPosts();
    }
}
