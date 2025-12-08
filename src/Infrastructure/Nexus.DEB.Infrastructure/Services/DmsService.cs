using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Dms;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace Nexus.DEB.Infrastructure.Services
{
    public class DmsService : LegacyApiServiceBase<DmsService>, IDmsService
    {
        protected override string HttpClientName => "DmsApi";

        public DmsService(
            IHttpClientFactory httpClientFactory,
            ILogger<DmsService> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
            : base(httpClientFactory, logger, httpContextAccessor, configuration)
        {
        }

        /// <summary>
        /// Adds a new document to a library.
        /// Sends multipart/form-data to the legacy API.
        /// </summary>
        public async Task<DmsDocumentResponse?> AddDocumentAsync(
            Guid libraryId,
            IFormFile file,
            DmsDocumentMetadata metadata)
        {
            var requestUri = $"api/libraries/{libraryId}/document";

            try
            {
                Logger.LogInformation(
                    "Adding document to library {LibraryId}: {FileName} for entity {EntityId}",
                    libraryId, file.FileName, metadata.EntityId);

                // Validate metadata
                if (metadata.EntityId == Guid.Empty)
                {
                    throw new ArgumentException("EntityId is required", nameof(metadata));
                }

                var validDocTypes = new[] { "document", "note" };
                if (!validDocTypes.Contains(metadata.DocumentType.ToLower()))
                {
                    throw new ArgumentException(
                        "DocumentType must be 'document' or 'note'",
                        nameof(metadata));
                }

                // Create multipart/form-data content
                using var content = CreateMultipartContentAsync(file, metadata);

                // Create authenticated request with the Forms Auth cookie
                var request = CreateAuthenticatedRequest(HttpMethod.Post, requestUri);
                request.Content = content;

                // Send the request
                var response = await HttpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    Logger.LogWarning(
                        "AddDocument forbidden for library {LibraryId}",
                        libraryId);
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Logger.LogError(
                        "AddDocument failed with status {StatusCode}: {ErrorContent}",
                        response.StatusCode, errorContent);
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<DmsDocumentResponse>(JsonOptions);

                Logger.LogInformation(
                    "Successfully added document {DocumentId} to library {LibraryId}",
                    result?.DocumentId, libraryId);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error adding document to library {LibraryId}", libraryId);
                throw;
            }
        }

        ///// <summary>
        ///// Updates an existing document in a library.
        ///// Sends multipart/form-data to the legacy API.
        ///// </summary>
        //public async Task<DmsDocumentResponse?> UpdateDocumentAsync(
        //    Guid libraryId,
        //    Guid documentId,
        //    IFormFile file,
        //    string? metadata = null)
        //{
        //    var requestUri = $"api/libraries/{libraryId}/document/{documentId}";

        //    try
        //    {
        //        Logger.LogInformation(
        //            "Updating document {DocumentId} in library {LibraryId}: {FileName} ({FileSize} bytes)",
        //            documentId, libraryId, file.FileName, file.Length);

        //        // Create multipart/form-data content
        //        using var content = await CreateMultipartContentAsync(file, metadata);

        //        // Create authenticated request with the Forms Auth cookie
        //        var request = CreateAuthenticatedRequest(HttpMethod.Post, requestUri);
        //        request.Content = content;

        //        // Send the request
        //        var response = await HttpClient.SendAsync(request);

        //        if (response.StatusCode == HttpStatusCode.Forbidden)
        //        {
        //            Logger.LogWarning(
        //                "UpdateDocument forbidden for library {LibraryId}, document {DocumentId}",
        //                libraryId, documentId);
        //            return null;
        //        }

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            var errorContent = await response.Content.ReadAsStringAsync();
        //            Logger.LogError(
        //                "UpdateDocument failed with status {StatusCode}: {ErrorContent}",
        //                response.StatusCode, errorContent);
        //            return null;
        //        }

        //        var result = await response.Content.ReadFromJsonAsync<DmsDocumentResponse>(JsonOptions);

        //        Logger.LogInformation(
        //            "Successfully updated document {DocumentId} in library {LibraryId}",
        //            documentId, libraryId);

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError(ex, "Error updating document {DocumentId} in library {LibraryId}",
        //            documentId, libraryId);
        //        throw;
        //    }
        //}

        /// <summary>
        /// Deletes a document from a library.
        /// </summary>
        public async Task<bool> DeleteDocumentAsync(Guid libraryId, Guid documentId)
        {
            var requestUri = $"api/libraries/{libraryId}/document/{documentId}";

            return await SendAuthenticatedValidationRequestAsync(
                HttpMethod.Delete,
                requestUri,
                operationName: $"DeleteDocument {documentId} from library {libraryId}");
        }

        /// <summary>
        /// Gets document metadata (without file data).
        /// </summary>
        public async Task<DmsDocument?> GetDocumentAsync(
            Guid libraryId,
            Guid documentId,
            int? version = null)
        {
            var requestUri = $"api/libraries/{libraryId}/document/{documentId}";
            if (version.HasValue)
            {
                requestUri += $"?version={version.Value}";
            }

            return await SendAuthenticatedRequestAsync<DmsDocument>(
                HttpMethod.Get,
                requestUri,
                operationName: $"GetDocument {documentId} from library {libraryId}");
        }

        /// <summary>
        /// Gets the document file data for download.
        /// Returns the raw bytes and metadata needed for file download.
        /// </summary>
        public async Task<DmsDocumentFile?> GetDocumentFileAsync(
            Guid libraryId,
            Guid documentId,
            int? version = null)
        {
            var requestUri = $"api/libraries/{libraryId}/document/{documentId}/file";
            if (version.HasValue)
            {
                requestUri += $"?version={version.Value}";
            }

            try
            {
                Logger.LogInformation(
                    "Getting document file {DocumentId} from library {LibraryId}",
                    documentId, libraryId);

                // Create authenticated request
                var request = CreateAuthenticatedRequest(HttpMethod.Get, requestUri);
                var response = await HttpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Logger.LogWarning(
                        "Document file {DocumentId} not found in library {LibraryId}",
                        documentId, libraryId);
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Logger.LogError(
                        "GetDocumentFile failed with status {StatusCode}: {ErrorContent}",
                        response.StatusCode, errorContent);
                    return null;
                }

                // Extract file information from headers
                var fileName = "download";
                if (response.Content.Headers.ContentDisposition?.FileName != null)
                {
                    fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');
                }

                var mimeType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

                // Read the file bytes
                var fileData = await response.Content.ReadAsByteArrayAsync();

                Logger.LogInformation(
                    "Successfully retrieved document file {DocumentId}: {FileName} ({FileSize} bytes)",
                    documentId, fileName, fileData.Length);

                return new DmsDocumentFile
                {
                    DocumentId = documentId,
                    FileName = fileName,
                    MimeType = mimeType,
                    FileData = fileData
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Error getting document file {DocumentId} from library {LibraryId}",
                    documentId, libraryId);
                throw;
            }
        }

        /// <summary>
        /// Gets a list of documents for an entity.
        /// </summary>
        public async Task<ICollection<DmsDocumentListItem>?> GetDocumentListByEntityAsync(
            Guid libraryId,
            Guid entityId)
        {
            // Note: The legacy API uses a GET
            // The route is not clearly defined in the provided controller (commented out)
            // Assuming: api/libraries/{libraryId}/documents/{entityId}
            var requestUri = $"api/libraries/{libraryId}/entity/{entityId}";

            return await SendAuthenticatedRequestAsync<ICollection<DmsDocumentListItem>>(
                HttpMethod.Get,
                requestUri,
                operationName: $"GetDocumentListByEntity for entity {entityId} in library {libraryId}");
        }

        /// <summary>
        /// Gets document history/version information.
        /// </summary>
        public async Task<ICollection<DmsDocumentHistoryItem>?> GetDocumentHistoryAsync(
            Guid libraryId,
            Guid documentId)
        {
            var requestUri = $"api/libraries/{libraryId}/document/{documentId}/history";

            return await SendAuthenticatedRequestAsync<ICollection<DmsDocumentHistoryItem>>(
                HttpMethod.Get,
                requestUri,
                operationName: $"GetDocumentHistory for document {documentId} in library {libraryId}");
        }

        /// <summary>
        /// Gets the count of documents for an entity.
        /// </summary>
        public async Task<int> GetEntityDocumentCountAsync(Guid libraryId, Guid entityId)
        {
            var requestUri = $"api/libraries/{libraryId}/entity/{entityId}/count";

            var result = await SendAuthenticatedRequestAsync<DmsDocumentCount>(
                HttpMethod.Get,
                requestUri,
                operationName: $"GetEntityDocumentCount for entity {entityId} in library {libraryId}");

            return result?.DocCount ?? 0;
        }

        /// <summary>
        /// Creates multipart/form-data content for file uploads.
        /// Includes the file and optional metadata.
        /// </summary>
        private static MultipartFormDataContent CreateMultipartContentAsync(
            IFormFile file,
            DmsDocumentMetadata metadata)
        {
            var content = new MultipartFormDataContent();

            // Add the file
            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                file.ContentType ?? "application/octet-stream");

            content.Add(fileContent, "file", file.FileName);

            // Add individual metadata fields as the legacy API expects them
            content.Add(new StringContent(metadata.EntityId.ToString(), Encoding.UTF8), "entityId");
            content.Add(new StringContent(metadata.DocumentType, Encoding.UTF8), "documentType");

            if (!string.IsNullOrWhiteSpace(metadata.Title))
            {
                content.Add(new StringContent(metadata.Title, Encoding.UTF8), "title");
            }

            if (!string.IsNullOrWhiteSpace(metadata.Description))
            {
                content.Add(new StringContent(metadata.Description, Encoding.UTF8), "description");
            }

            if (!string.IsNullOrWhiteSpace(metadata.Author))
            {
                content.Add(new StringContent(metadata.Author, Encoding.UTF8), "author");
            }

            return content;
        }
    }
}
