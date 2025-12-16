using HotChocolate.Resolvers;
using Nexus.DEB.Api.Security;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Interfaces;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Api.GraphQL
{
    public class EntityTypeResolver
    {
        public async Task<bool> GetCanEditAsync([Parent] IEntityType entity, PawsStatusDataLoader dataloader, IResolverContext resolverContext, CancellationToken cancellationToken)
        {
            var canEdit = false;
            var debUser = new DebUser(resolverContext.GetUser());
            var editCapability = DebHelper.Capabilites.EditCapabilityByEntityType[entity.EntityTypeTitle];

            switch(entity.EntityTypeTitle)
            {
                case EntityTypes.Task:
                    if (debUser.Capabilities.Contains(editCapability))
                        canEdit = true;
                    break;

                default:
                    var pseudostateTitle = await dataloader.LoadAsync(entity.EntityId, cancellationToken);

                    if (!string.IsNullOrEmpty(pseudostateTitle) && DebHelper.Paws.States.AllEditableStates.Contains(pseudostateTitle) && debUser.Capabilities.Contains(editCapability))
                        canEdit = true;
                    break;
            }

            return canEdit;
        }
    }
}
