using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Infrastructure.Services
{
    public class SampleUserValidationService : IUserValidationService
    {
        public async Task<CisUser?> ValidateCredentialsAsync(string username, string password)
        {
            var user = new CisUser()
            {
                UserId = Guid.NewGuid(),
                Posts = new List<CisPost>()
                {
                    new CisPost() {
                        PostId = Guid.NewGuid(),
                        Title = "example post 1"
                    },
                    new CisPost() {
                        PostId = Guid.NewGuid(),
                        Title = "example post 2"
                    }

                }
            };

            return user;
        }
    }
}
