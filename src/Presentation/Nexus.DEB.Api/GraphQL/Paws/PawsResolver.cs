using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Api.GraphQL.Paws
{
    public class PawsResolver
    {
        public async Task<string?> GetCurrentPawsStatusAsync([Parent] IEntity entity, PawsStatusDataLoader dataLoader, CancellationToken cancellationToken)
        {
            return await dataLoader.LoadAsync(entity.Id, cancellationToken);
        }
    }
}
