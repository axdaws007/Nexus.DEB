using HotChocolate.Resolvers;
using Nexus.DEB.Api.Security;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Api.GraphQL
{
    public class CanUpVersionStandardVersionResolver
    {
        public async Task<bool> GetCanUpVersionAsync(
            [Parent] IEntity entity, 
            PawsStatusDataLoader pawsDataLoader,
            HasDraftStandardVersionsDataLoader hasDraftDataLoader,
            IResolverContext resolverContext, 
            IDebService debService,
            CancellationToken cancellationToken)
        {
            bool canUpVersion = false;
            var debUser = new DebUser(resolverContext.GetUser());

            var pseudostateTitle = await pawsDataLoader.LoadAsync(entity.EntityId, cancellationToken);

            var hasUpVersionCapability = debUser.Capabilities.Contains(DebHelper.Capabilites.CanUpVersionStdVersion);

            var hasOtherDraftVersions = await hasDraftDataLoader.LoadAsync(entity.EntityId, cancellationToken);

            if (!string.IsNullOrEmpty(pseudostateTitle) && pseudostateTitle == DebHelper.Paws.States.Active && !hasOtherDraftVersions && hasUpVersionCapability)
                canUpVersion = true;

            return canUpVersion;
        }
    }
}
