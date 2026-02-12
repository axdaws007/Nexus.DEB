using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Dms;
using Nexus.DEB.Domain;

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
           IDmsService dmsService)
        {
            try
            {
                DebHelper.Dms.Libraries.Validator.ValidateOrThrow(library);
            }
            catch (Exception ex)
            {
                throw ExceptionHelper.BuildException(ex);
            }

            var libraryId = applicationSettingsService.GetLibraryId(library);

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

            await debService.UpdateLinkedCommonDocumentsAsync(entityId, libraryId, availableDocumentIds, idsToAdd, idsToRemove, addAll, removeAll);

            return await debService.GetStatementDetailByIdAsync(entityId, cancellationToken);
        }
    }
}
