using Microsoft.AspNetCore.Http;
using Nexus.DEB.Application.Common.Models.Dms;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IDmsService
    {
        /// <summary>
        /// Adds a new document to a library.
        /// </summary>
        /// <param name="libraryId">The library ID</param>
        /// <param name="file">The file to upload</param>
        /// <param name="metadata">Document metadata (optional JSON string from form data)</param>
        /// <returns>The created document response</returns>
        Task<DmsDocumentResponse?> AddDocumentAsync(Guid libraryId, IFormFile file, DmsDocumentMetadata metadata);

        /// <summary>
        /// Updates an existing document in a library.
        /// </summary>
        /// <param name="libraryId">The library ID</param>
        /// <param name="documentId">The document ID to update</param>
        /// <param name="file">The file to upload</param>
        /// <param name="metadata">Document metadata (optional JSON string from form data)</param>
        /// <returns>The updated document response</returns>
        Task<DmsDocumentResponse?> UpdateDocumentAsync(Guid libraryId, Guid documentId, IFormFile? file, DmsDocumentMetadata? metadata = null);

        /// <summary>
        /// Deletes a document from a library.
        /// </summary>
        /// <param name="libraryId">The library ID</param>
        /// <param name="documentId">The document ID to delete</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteDocumentAsync(Guid libraryId, Guid documentId);

        /// <summary>
        /// Gets document metadata (without file data).
        /// </summary>
        /// <param name="libraryId">The library ID</param>
        /// <param name="documentId">The document ID</param>
        /// <param name="version">Optional version number</param>
        /// <returns>Document metadata</returns>
        Task<DmsDocument?> GetDocumentAsync(Guid libraryId, Guid documentId, int? version = null);

        /// <summary>
        /// Gets the document file data for download.
        /// </summary>
        /// <param name="libraryId">The library ID</param>
        /// <param name="documentId">The document ID</param>
        /// <param name="version">Optional version number</param>
        /// <returns>Document with file data</returns>
        Task<DmsDocumentFile?> GetDocumentFileAsync(Guid libraryId, Guid documentId, int? version = null);

        /// <summary>
        /// Gets a list of documents for an entity.
        /// </summary>
        /// <param name="libraryId">The library ID</param>
        /// <param name="entityId">The entity ID</param>
        /// <returns>list of documents</returns>
        Task<ICollection<DmsDocumentListItem>?> GetDocumentListByEntityAsync(Guid libraryId, Guid entityId);

        /// <summary>
        /// Gets document history/version information.
        /// </summary>
        /// <param name="libraryId">The library ID</param>
        /// <param name="documentId">The document ID</param>
        /// <param name="parameters">Paging and sorting parameters</param>
        /// <returns>Paginated list of document versions</returns>
        Task<ICollection<DmsDocumentHistoryItem>?> GetDocumentHistoryAsync(Guid libraryId, Guid documentId);

        /// <summary>
        /// Gets the count of documents for an entity.
        /// </summary>
        /// <param name="libraryId">The library ID</param>
        /// <param name="entityId">The entity ID</param>
        /// <returns>Document count</returns>
        Task<int> GetEntityDocumentCountAsync(Guid libraryId, Guid entityId);

        Task AddDocumentUploadedAuditRecordAsync(Guid documentId, Guid entityId);
		Task AddDocumentDownloadedAuditRecordAsync(Guid documentId, Guid entityId);
		Task AddDocumentUpdatedAuditRecordAsync(Guid documentId, Guid entityId);
        Task AddDocumentDeletedAuditRecordAsync(Guid libraryId, Guid documentId);

        /// <summary>
        /// Gets a list of documents for an entity.
        /// </summary>
        /// <param name="libraryId">The library ID</param>
        /// <param name="filters">Set of filters</param>
        /// <returns>list of documents</returns>
        Task<ICollection<DmsCommonDocumentListItem>?> GetCommonDocumentListAsync(Guid libraryId, DmsCommonDocumentListFilters filters);

        Task<ICollection<DmsDocumentTypeItem>?> GetDocumentTypesAsync(Guid libraryId, Guid groupId);
    }
}
