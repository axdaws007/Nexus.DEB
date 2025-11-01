using HotChocolate.Data.Filters;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Api.GraphQL.Paws
{
    public class PawsResolver
    {
        public async Task<string?> GetCurrentPawsStatusAsync([Parent] IEntity entity, IPawsService pawsService, CancellationToken cancellationToken)
        {
            return pawsService.GetStatusForEntity(entity.Id);
        }
    }
}
