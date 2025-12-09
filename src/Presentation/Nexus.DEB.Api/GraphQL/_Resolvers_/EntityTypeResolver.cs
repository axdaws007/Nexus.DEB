using HotChocolate.Resolvers;
using Nexus.DEB.Api.Security;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Interfaces;

namespace Nexus.DEB.Api.GraphQL
{
    public class EntityTypeResolver
    {
        public async Task<bool> GetCanEditAsync([Parent] IEntityType entity, PawsStatusDataLoader dataloader, IResolverContext resolverContext, CancellationToken cancellationToken)
        {
            var canEdit = false;
            var debUser = new DebUser(resolverContext.GetUser());

            var pseudostateTitle = await dataloader.LoadAsync(entity.EntityId, cancellationToken);

            var editCapability = DebHelper.Capabilites.EditCapabilityByEntityType[entity.EntityTypeTitle];

            if (!string.IsNullOrEmpty(pseudostateTitle) && DebHelper.Paws.States.AllEditableStates.Contains(pseudostateTitle) && debUser.Capabilities.Contains(editCapability))
                canEdit = true;

            return canEdit;
        }
    }
}
