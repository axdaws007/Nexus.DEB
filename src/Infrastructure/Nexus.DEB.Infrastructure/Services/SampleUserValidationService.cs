using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Infrastructure.Services
{
    public class SampleUserValidationService : IUserValidationService
    {
        public async Task<CisUser?> ValidateCredentialsAsync(string username, string password)
        {

        }
    }
}
