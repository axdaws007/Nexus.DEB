using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IUserValidationService
    {
        Task<CisUser?> ValidateCredentialsAsync(string username, string password);

        Task<bool> ValidatePostAsync(Guid userId, Guid postId, string authCookie);
    }
}
