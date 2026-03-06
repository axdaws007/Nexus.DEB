using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Dms;
using Nexus.DEB.Application.Common.Models.Events;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Interfaces;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Infrastructure.Helpers;

namespace Nexus.DEB.Api.GraphQL
{
    [MutationType]
    public static class DmsMutations
    {
        [Authorize(Policy = DebHelper.Policies.CanDeleteDocuments)]
        [UseMutationConvention(Disable = true)]
        public static async Task<bool> DeleteDocument(
           string library,
           Guid documentId,
           IApplicationSettingsService applicationSettingsService,
           IDebService debService,
           IDmsService dmsService,
           IDomainEventPublisher eventPublisher,
           IResolverContext resolverContext,
           CancellationToken cancellationToken)
        {
            var debUser = new DebUser(resolverContext.GetUser());

            try
            {
                DebHelper.Dms.Libraries.Validator.ValidateOrThrow(library);
            }
            catch (Exception ex)
            {
                throw ExceptionHelper.BuildException(ex);
            }

            var libraryId = applicationSettingsService.GetLibraryId(library);

            var documentTitle = string.Empty;
            var fileName = string.Empty;
            var documentFound = false;

            switch(library)
            {
                case DebHelper.Dms.Libraries.CommonDocuments:
                    var commonDocument = await dmsService.GetCommonLibraryDocumentAsync(libraryId, documentId);

                    if (commonDocument != null)
                    {
                        documentTitle = commonDocument.Title;
                        fileName = commonDocument.FileName;
                        documentFound = true;
                    }
                    break;

                case DebHelper.Dms.Libraries.DebDocuments:
                    var debDocument = await dmsService.GetDebLibraryDocumentAsync(libraryId, documentId);

                    if (debDocument != null)
                    {
                        documentTitle = debDocument.Title;
                        fileName = debDocument.FileName;
                        documentFound = true;
                    }
                    break;
            }

            if (!documentFound)
            {
                throw ExceptionHelper.BuildException(new FileNotFoundException($"Document ID '{documentId}' in library '{libraryId}' does not exist."));
            }

            var affectedEntities = await debService.GetLinkedEntitiesForDocumentAsync(libraryId, documentId, cancellationToken);

            if (affectedEntities.Count > 0)
            {
                try
                {
                    await debService.DeleteLinkedDocumentAsync(libraryId, documentId, cancellationToken);
                }
                catch (Exception ex)
                {
                    throw ExceptionHelper.BuildException(ex);
                }

                var fileReference = $"'{documentTitle}' ({fileName})";
                var comment = $"Removing link to document {fileReference} due to it's deletion.";

                var changeRecordItem = new ChangeRecordItemParameters("EntityDocumentLinking", "Removed document link", fileReference, string.Empty);

                await debService.CreateBatchOfChangeRecordsAsync(affectedEntities, comment, debUser.UserDetails, [changeRecordItem], cancellationToken);

                var entities = await debService.GetEntityHeadsAsync(affectedEntities, cancellationToken);

                foreach (var entity in entities)
                {
                    await eventPublisher.PublishAsync(new ChildEntitySavedEvent
                    {
                        ParentEntityType = entity.Value.EntityTypeTitle,
                        ParentEntityId = entity.Key,
                        ChildEntityType = "EntityDocumentLinking",
                        EventContext = comment
                    }, cancellationToken);
                }
            }

            return await dmsService.DeleteDocumentAsync(libraryId, documentId);
        }

        [Authorize(Policy = DebHelper.Policies.CanCreateOrEditSoC)]
        public static async Task<StatementDetail?> UpdateLinkedCommonDocumentsAsync(
            Guid entityId, 
            ICollection<Guid> idsToAdd, 
            ICollection<Guid> idsToRemove, 
            bool addAll, 
            bool removeAll, 
            IDebService debService,
            IDmsService dmsService,
            IApplicationSettingsService applicationSettingsService,
            CancellationToken cancellationToken)
        {
            var libraryId = applicationSettingsService.GetLibraryId(DebHelper.Dms.Libraries.CommonDocuments);

            var commonLibraryDocuments = await dmsService.GetCommonDocumentListAsync(libraryId, new DmsCommonDocumentListFilters());

            var availableDocumentIds = commonLibraryDocuments.Select(x => x.ActionData.ID).ToList();

            var linkDiff = GetDocumentLinksDiff(entityId, idsToAdd, idsToRemove, addAll, removeAll, availableDocumentIds, debService);

			var isSuccessful = await debService.UpdateLinkedCommonDocumentsAsync(entityId, libraryId, linkDiff.toDelete, linkDiff.toInsert);

            if (isSuccessful)
            {
                if (linkDiff.toDelete != null && linkDiff.toDelete.Count > 0) 
                {
                    await LogLinkedCommonDocUpdateInChangeHistory(entityId, libraryId, false, linkDiff.toDelete, debService, dmsService, cancellationToken);
                }
				if (linkDiff.toInsert != null && linkDiff.toInsert.Count > 0)
				{
					await LogLinkedCommonDocUpdateInChangeHistory(entityId, libraryId, true, linkDiff.toInsert, debService, dmsService, cancellationToken);
				}
			}

            return await debService.GetStatementDetailByIdAsync(entityId, cancellationToken);
        }

        private static (List<Guid>? toDelete, List<Guid>? toInsert) GetDocumentLinksDiff(
            Guid entityId,
			ICollection<Guid> idsToAdd,
			ICollection<Guid> idsToRemove,
			bool addAll,
			bool removeAll,
            List<Guid>? availableDocumentIds,
			IDebService debService)
        {
			// Get current state
			var existingIds = new HashSet<Guid>(
				debService.GetLinkedDocumentsForEntityAndContext(entityId, EntityDocumentLinkingContexts.CommonEvidence)
					.Select(x => x.DocumentId));

			// Calculate desired state
			HashSet<Guid> desiredIds;

			if (addAll)
			{
				desiredIds = new HashSet<Guid>(availableDocumentIds);
				desiredIds.ExceptWith(idsToRemove);
			}
			else if (removeAll)
			{
				desiredIds = new HashSet<Guid>(idsToAdd);
			}
			else
			{
				desiredIds = new HashSet<Guid>(existingIds);
				desiredIds.ExceptWith(idsToRemove);
				desiredIds.UnionWith(idsToAdd);
			}

			// Diff: what actually needs to change
			var toDelete = existingIds.Except(desiredIds).ToList();
			var toInsert = desiredIds.Except(existingIds).ToList();

            return (toDelete, toInsert);
		}

		private static async System.Threading.Tasks.Task LogLinkedCommonDocUpdateInChangeHistory(
            Guid entityId, 
            Guid libraryId, 
            bool forInsert, 
            List<Guid> documentIds, 
            IDebService debService, 
            IDmsService dmsService, 
            CancellationToken cancellationToken)
		{
			string oldValue = "";
			string newValue = "";

            var docs = await dmsService.GetDocumentListByDocumentIdsAsync(libraryId, documentIds);

            if (forInsert)
            {
                newValue = string.Join("\n", docs.Select(s => s.Title));
            }
            else
            {
				oldValue = string.Join("\n", docs.Select(s => s.Title));
			}

            await debService.AddChangeRecordItem(entityId, "EntityDocumentLinking", (forInsert ? "Document link(s) added" : "Document link(s) removed"), oldValue, newValue, cancellationToken);
		}
	}
}
