using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ILoginService
    {
        Task<Result<LoginResponse>> SignInAsync(string username, string password, bool rememberMe = false);
    }
}
