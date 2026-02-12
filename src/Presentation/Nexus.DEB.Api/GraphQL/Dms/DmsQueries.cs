using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Dms;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Infrastructure.Services;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class DmsQueries
    {
        /// <summary>
        /// Gets a single document from the DEB Documents library.
        /// </summary>
        /// <param name="documentId">The document ID</param>
        /// <param name="version">Optional version number</param>
        /// <param name="applicationSettingsService">Settings service</param>
        /// <param name="dmsService">DMS service</param>
        /// <returns>The document details</returns>
        [Authorize]
        public static async Task<DebDmsDocument?> GetDebLibraryDocument(
            Guid documentId,
            int? version,
            [Service] IApplicationSettingsService applicationSettingsService,
            [Service] IDmsService dmsService)
        {
            var libraryId = applicationSettingsService.GetLibraryId(DebHelper.Dms.Libraries.DebDocuments);
            return await dmsService.GetDebLibraryDocumentAsync(libraryId, documentId, version);
        }

        /// <summary>
        /// Gets a single document from the Common Documents library.
        /// </summary>
        /// <param name="documentId">The document ID</param>
        /// <param name="version">Optional version number</param>
        /// <param name="applicationSettingsService">Settings service</param>
        /// <param name="dmsService">DMS service</param>
        /// <returns>The document details</returns>
        [Authorize]
        public static async Task<CommonDmsDocument?> GetCommonLibraryDocument(
            Guid documentId,
            int? version,
            [Service] IApplicationSettingsService applicationSettingsService,
            [Service] IDmsService dmsService)
        {
            var libraryId = applicationSettingsService.GetLibraryId(DebHelper.Dms.Libraries.CommonDocuments);
            return await dmsService.GetCommonLibraryDocumentAsync(libraryId, documentId, version);
        }

        /// <summary>
        /// Gets a list of documents for an entity with paging and sorting.
        /// </summary>
        /// <param name="libraryId">The library ID</param>
        /// <param name="entityId">The entity ID</param>
        /// <param name="parameters">Paging and sorting parameters</param>
        /// <param name="dmsService">Injected DMS service</param>
        /// <returns>Paginated list of documents</returns>
        [Authorize]
        [UseOffsetPaging]
        [UseSorting]
        public static async Task<ICollection<DmsDocumentListItem>?> GetDocumentListForEntity(
            string library,
            Guid entityId,
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

            return await dmsService.GetDocumentListByEntityAsync(libraryId, entityId);
        }

        [Authorize]
        [UseOffsetPaging]
        [UseSorting]
        public static async Task<ICollection<DmsCommonDocumentListItem>?> GetCommonDocumentList(
            DmsCommonDocumentListFilters filters,
            IApplicationSettingsService applicationSettingsService,
            IDmsService dmsService)
        {
            var libraryId = applicationSettingsService.GetLibraryId(DebHelper.Dms.Libraries.CommonDocuments);

            return await dmsService.GetCommonDocumentListAsync(libraryId, filters);
        }

        [Authorize]
        public static async Task<ICollection<DmsDocumentTypeItem>?> GetDefaultDocumentTypesForLibrary(
            string library,
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
            var groupId = applicationSettingsService.GetDefaultDocumentTypeGroupId(library);

            return await dmsService.GetDocumentTypesAsync(libraryId, groupId);
        }

        /// <summary>
        /// Gets document history/version information.
        /// </summary>
        /// <param name="libraryId">The library ID</param>
        /// <param name="documentId">The document ID</param>
        /// <param name="parameters">Paging and sorting parameters</param>
        /// <param name="dmsService">Injected DMS service</param>
        /// <returns>Paginated list of document versions</returns>
        [Authorize]
        public static async Task<ICollection<DmsDocumentHistoryItem>?> GetDocumentHistory(
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

            return await dmsService.GetDocumentHistoryAsync(libraryId, documentId);
        }

        /// <summary>
        /// Gets the count of documents for an entity.
        /// </summary>
        /// <param name="libraryId">The library ID</param>
        /// <param name="entityId">The entity ID</param>
        /// <param name="dmsService">Injected DMS service</param>
        /// <returns>Document count</returns>
        [Authorize]
        public static async Task<int> GetDocumentCountForEntity(
            string library,
            Guid entityId,
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

            return await dmsService.GetEntityDocumentCountAsync(libraryId, entityId);
        }

        [Authorize]
        public static async Task<DmsSettings?> GetDmsSettings(IDmsService dmsService)
            => await dmsService.GetSettingsAsync();

        [Authorize]
        [UseOffsetPaging]
        [UseSorting]
        public static async Task<ICollection<DmsDocumentListItem>?> GetLinkedCommonDocuments(
            Guid entityId,
            IDebService debService,
            IDmsService dmsService,
            IApplicationSettingsService applicationSettingsService,
            CancellationToken cancellationToken)
        {
            var libraryId = applicationSettingsService.GetLibraryId(DebHelper.Dms.Libraries.CommonDocuments);

            var listOfDocuments = debService.GetLinkedDocumentsForEntityAndContext(entityId, EntityDocumentLinkingContexts.CommonEvidence);

            var commonDocumentIds = listOfDocuments.Where(x => x.LibraryId == libraryId).Select(x => x.DocumentId).ToList();

            return await dmsService.GetDocumentListByDocumentIdsAsync(libraryId, commonDocumentIds);
        }
    }
}
