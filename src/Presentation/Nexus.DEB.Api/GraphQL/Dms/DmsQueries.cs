using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Dms;
using Nexus.DEB.Domain;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class DmsQueries
    {
        /// <summary>
        /// Gets document metadata (without file data).
        /// </summary>
        /// <param name="libraryId">The library ID</param>
        /// <param name="documentId">The document ID</param>
        /// <param name="version">Optional version number</param>
        /// <param name="dmsService">Injected DMS service</param>
        /// <returns>Document metadata or null if not found</returns>
        [Authorize]
        public static async Task<DmsDocument?> GetDocument(
            string library,
            Guid documentId,
            int? version,
            IApplicationSettingsService applicationSettingsService,
            IDmsService dmsService)
        {
            try
            {
                DebHelper.Dms.Libraries.Validator.ValidateOrThrow(library);
            }
            catch(Exception ex)
            {
                throw ExceptionHelper.BuildException(ex);
            }

            var libraryId = applicationSettingsService.GetLibraryId(library);

            return await dmsService.GetDocumentAsync(libraryId, documentId, version);
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
    }
}
