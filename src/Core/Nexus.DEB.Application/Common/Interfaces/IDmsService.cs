using Microsoft.AspNetCore.Http;
using Nexus.DEB.Application.Common.Models.Dms;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IDmsService
    {
        #region Document Operations

        /// <summary>
        /// Adds a new document to a library.
        /// </summary>
        Task<DmsDocumentResponse?> AddDocumentAsync(Guid libraryId, IFormFile file, DmsDocumentMetadata metadata);

        /// <summary>
        /// Updates an existing document in a library.
        /// </summary>
        Task<DmsDocumentResponse?> UpdateDocumentAsync(Guid libraryId, Guid documentId, IFormFile? file, DmsDocumentMetadata? metadata = null);

        /// <summary>
        /// Deletes a document from a library.
        /// </summary>
        Task<bool> DeleteDocumentAsync(Guid libraryId, Guid documentId);

        /// <summary>
        /// Gets the document file data for download.
        /// </summary>
        Task<DmsDocumentFile?> GetDocumentFileAsync(Guid libraryId, Guid documentId, int? version = null);

        #endregion


        #region DEB Documents Library

        /// <summary>
        /// Gets a document from the deb-documents library.
        /// </summary>
        Task<DebDmsDocument?> GetDebLibraryDocumentAsync(Guid libraryId, Guid documentId, int? version = null);

        /// <summary>
        /// Gets a list of documents for an entity from the deb-documents library.
        /// </summary>
        Task<ICollection<DmsDocumentListItem>?> GetDocumentListByEntityAsync(Guid libraryId, Guid entityId);

        #endregion


        #region Common Documents Library

        /// <summary>
        /// Gets a document from the common-documents library.
        /// </summary>
        Task<CommonDmsDocument?> GetCommonLibraryDocumentAsync(Guid libraryId, Guid documentId, int? version = null);


        /// <summary>
        /// Gets a list of documents for an entity.
        /// </summary>
        /// <param name="libraryId">The library ID</param>
        /// <param name="filters">Set of filters</param>
        /// <returns>list of documents</returns>
        Task<ICollection<DmsCommonDocumentListItem>?> GetCommonDocumentListAsync(Guid libraryId, DmsCommonDocumentListFilters filters);

        #endregion


        #region Shared Operations

        /// <summary>
        /// Gets DMS settings
        /// </summary>
        Task<DmsSettings> GetSettingsAsync();

        /// <summary>
        /// Gets document metadata (without file data).
        /// </summary>
        Task<DmsDocument?> GetDocumentAsync(Guid libraryId, Guid documentId, int? version = null);

        /// <summary>
        /// Gets document history/version information.
        /// </summary>
        Task<ICollection<DmsDocumentHistoryItem>?> GetDocumentHistoryAsync(Guid libraryId, Guid documentId);

        /// <summary>
        /// Gets the count of documents for an entity.
        /// </summary>
        Task<int> GetEntityDocumentCountAsync(Guid libraryId, Guid entityId);

        /// <summary>
        /// Gets document types for a library and group.
        /// </summary>
        Task<ICollection<DmsDocumentTypeItem>?> GetDocumentTypesAsync(Guid libraryId, Guid groupId);

        #endregion


        #region Audit Records

        Task AddDocumentUploadedAuditRecordAsync(Guid documentId, Guid? entityId);
        Task AddDocumentDownloadedAuditRecordAsync(Guid documentId, Guid? entityId);
        Task AddDocumentUpdatedAuditRecordAsync(Guid documentId, Guid? entityId);
        Task AddDocumentDeletedAuditRecordAsync(Guid libraryId, Guid documentId);

        #endregion
    }
}
