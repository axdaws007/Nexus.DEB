using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Extensions;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Dms;
using Nexus.DEB.Domain.Models.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace Nexus.DEB.Infrastructure.Services
{
    public class DmsService : LegacyApiServiceBase<DmsService>, IDmsService
    {
        protected override string HttpClientName => "DmsApi";
        private readonly IDebService _debService;
		private readonly IAuditService _auditService;
		private readonly ICurrentUserService _currentUserService;

		public DmsService(
            IHttpClientFactory httpClientFactory,
            ILogger<DmsService> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IDebService debService,
            IAuditService auditService,
            ICurrentUserService currentUserService)
            : base(httpClientFactory, logger, httpContextAccessor, configuration)
        {
			_debService = debService;
			_auditService = auditService;
			_currentUserService = currentUserService;
		}

        #region Document Operations

        /// <summary>
        /// Adds a document to a library.
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
                // Log with entityId if available
                var entityId = metadata.GetValueOrDefault("entityId") ?? "not specified";
                Logger.LogInformation(
                    "Adding document to library {LibraryId}: {FileName} for entity {EntityId}",
                    libraryId, file.FileName, entityId);

                // Create multipart/form-data content
                using var content = CreateMultipartContent(file, metadata);

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


        /// <summary>
        /// Updates an existing document in a library.
        /// Sends multipart/form-data to the legacy API.
        /// </summary>
        public async Task<DmsDocumentResponse?> UpdateDocumentAsync(
            Guid libraryId,
            Guid documentId,
            IFormFile? file,
            DmsDocumentMetadata metadata)
        {
            var requestUri = $"api/libraries/{libraryId}/document/{documentId}";

            try
            {
                Logger.LogInformation(
                    "Updating document {DocumentId} in library {LibraryId}: {FileName} ({FileSize} bytes)",
                    documentId, libraryId, file?.FileName ?? "no file", file?.Length ?? 0);

                // Create multipart/form-data content
                using var content = CreateMultipartContent(file, metadata);

                // Create authenticated request with the Forms Auth cookie
                var request = CreateAuthenticatedRequest(HttpMethod.Post, requestUri);
                request.Content = content;

                // Send the request
                var response = await HttpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    Logger.LogWarning(
                        "UpdateDocument forbidden for library {LibraryId}, document {DocumentId}",
                        libraryId, documentId);
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Logger.LogError(
                        "UpdateDocument failed with status {StatusCode}: {ErrorContent}",
                        response.StatusCode, errorContent);
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<DmsDocumentResponse>(JsonOptions);

                Logger.LogInformation(
                    "Successfully updated document {DocumentId} in library {LibraryId}",
                    documentId, libraryId);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating document {DocumentId} in library {LibraryId}",
                    documentId, libraryId);
                throw;
            }
        }

        /// <summary>
        /// Deletes a document from a library.
        /// </summary>
        public async Task<bool> DeleteDocumentAsync(Guid libraryId, Guid documentId)
        {
            var requestUri = $"api/libraries/{libraryId}/document/{documentId}";

            var successful = await SendAuthenticatedValidationRequestAsync(
                HttpMethod.Delete,
                requestUri,
                operationName: $"DeleteDocument {documentId} from library {libraryId}");

            if (successful)
            {
                await AddDocumentDeletedAuditRecordAsync(libraryId, documentId);
            }

            return successful;
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
        /// Creates multipart/form-data content for file uploads.
        /// Includes the file and metadata as a JSON string.
        /// </summary>
        private static MultipartFormDataContent CreateMultipartContent(
            IFormFile? file,
            DmsDocumentMetadata metadata)
        {
            var content = new MultipartFormDataContent();

            if (file != null)
            {
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                    file.ContentType ?? "application/octet-stream");

                content.Add(fileContent, "file", file.FileName);
            }

            // Pass through the raw JSON string to the legacy API
            if (!string.IsNullOrWhiteSpace(metadata.RawJson))
            {
                content.Add(new StringContent(metadata.RawJson, Encoding.UTF8), "metadata");
            }

            return content;
        }

        #endregion


        #region DEB Documents Library

        /// <summary>
        /// Gets a document from the deb-documents library.
        /// </summary>
        public async Task<DebDmsDocument?> GetDebLibraryDocumentAsync(Guid libraryId, Guid documentId, int? version = null)
        {
            var requestUri = version.HasValue
                ? $"api/libraries/{libraryId}/document/{documentId}?version={version}"
                : $"api/libraries/{libraryId}/document/{documentId}";

            try
            {
                Logger.LogInformation(
                    "Getting DEB document {DocumentId} from library {LibraryId}",
                    documentId, libraryId);

                var response = await SendAuthenticatedRequestAsync<DmsApiDocumentResponse>(
                    HttpMethod.Get,
                    requestUri,
                    operationName: $"GetDebDocument for document {documentId}");

                if (response == null)
                {
                    return null;
                }

                return MapToDebDocument(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting DEB document {DocumentId} from library {LibraryId}",
                    documentId, libraryId);
                throw;
            }
        }

        private static DebDmsDocument MapToDebDocument(DmsApiDocumentResponse response)
        {
            var document = new DebDmsDocument
            {
                DocumentId = response.DocumentId,
                FileName = response.FileName,
                FileSize = response.FileSize,
                MimeType = response.MimeType,
                DocumentOwnerId = response.DocumentOwnerId,
                DocumentOwner = response.DocumentOwner
            };

            // Extract common metadata
            if (response.Metadata != null)
            {
                document.Title = GetMetadataString(response.Metadata, "Title");
                document.Description = GetMetadataString(response.Metadata, "Description");
                document.Author = GetMetadataString(response.Metadata, "Author");
                document.DocumentType = GetMetadataString(response.Metadata, "DocumentType");
                document.UploadedBy = GetMetadataString(response.Metadata, "UploadedBy");
                document.UploadedDate = GetMetadataDateTime(response.Metadata, "UploadedDate");

                // Library-specific: EntityId
                document.EntityId = GetMetadataGuid(response.Metadata, "EntityId");
            }

            return document;
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

        #endregion


        #region Common Document Library

        public async Task<ICollection<DmsCommonDocumentListItem>?> GetCommonDocumentListAsync(
            Guid libraryId,
            DmsCommonDocumentListFilters filters)
        {
            // Note: The legacy API uses a GET
            // The route is not clearly defined in the provided controller (commented out)
            // Assuming: api/libraries/{libraryId}/documents/{entityId}
            var requestUri = $"api/libraries/{libraryId}";

            var uploadedFrom = filters.UploadedFrom.HasValue ? filters.UploadedFrom.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null;
            var uploadedTo = filters.UploadedTo.HasValue ? filters.UploadedTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue) : (DateTime?)null;

            var request = new
            {
                filters.StandardVersionIds,
                filters.SearchText,
                UploadedFrom = uploadedFrom,
                UploadedTo = uploadedTo,
                filters.Author,
                filters.DocumentTypes
            };

            // Create JSON content (JsonOptions from base class)
            var content = JsonContent.Create(request, options: JsonOptions);

            return await SendAuthenticatedRequestAsync<ICollection<DmsCommonDocumentListItem>>(
                HttpMethod.Post,
                requestUri,
                operationName: $"GetCommonDocumentList for library {libraryId}",
                content: content);
        }

        /// <summary>
        /// Gets a document from the common-documents library.
        /// </summary>
        public async Task<CommonDmsDocument?> GetCommonLibraryDocumentAsync(Guid libraryId, Guid documentId, int? version = null)
        {
            var requestUri = version.HasValue
                ? $"api/libraries/{libraryId}/document/{documentId}?version={version}"
                : $"api/libraries/{libraryId}/document/{documentId}";

            try
            {
                Logger.LogInformation(
                    "Getting Common document {DocumentId} from library {LibraryId}",
                    documentId, libraryId);

                var response = await SendAuthenticatedRequestAsync<DmsApiDocumentResponse>(
                    HttpMethod.Get,
                    requestUri,
                    operationName: $"GetCommonDocument for document {documentId}");

                if (response == null)
                {
                    return null;
                }

                return MapToCommonDocument(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting Common document {DocumentId} from library {LibraryId}",
                    documentId, libraryId);
                throw;
            }
        }

        private static CommonDmsDocument MapToCommonDocument(DmsApiDocumentResponse response)
        {
            var document = new CommonDmsDocument
            {
                DocumentId = response.DocumentId,
                FileName = response.FileName,
                FileSize = response.FileSize,
                MimeType = response.MimeType,
                DocumentOwnerId = response.DocumentOwnerId,
                DocumentOwner = response.DocumentOwner
            };

            // Extract common metadata
            if (response.Metadata != null)
            {
                document.Title = GetMetadataString(response.Metadata, "Title");
                document.Description = GetMetadataString(response.Metadata, "Description");
                document.Author = GetMetadataString(response.Metadata, "Author");
                document.DocumentType = GetMetadataString(response.Metadata, "DocumentType");
                document.UploadedBy = GetMetadataString(response.Metadata, "UploadedBy");
                document.UploadedDate = GetMetadataDateTime(response.Metadata, "UploadedDate");

                // Library-specific fields
                document.ExpiryDate = GetMetadataDateOnly(response.Metadata, "ExpiryDate");
                document.ReviewDate = GetMetadataDateOnly(response.Metadata, "ReviewDate");
                document.StandardVersionIds = GetMetadataString(response.Metadata, "StandardVersionIds");
            }

            return document;
        }

        #endregion


        #region Shared Operation

        /// <summary>
        /// Gets DMs settings
        /// </summary>
        public async Task<DmsSettings> GetSettingsAsync()
        {
            var requestUri = $"api/settings";

            var dmsSettings = await SendAuthenticatedRequestAsync<DmsSettings>(
                HttpMethod.Get,
                requestUri,
                operationName: $"GetSettings");

            if (dmsSettings == null) dmsSettings = new DmsSettings();

            return dmsSettings;
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

        public async Task<ICollection<DmsDocumentTypeItem>?> GetDocumentTypesAsync(
            Guid libraryId,
            Guid groupId)
        {
            var requestUri = $"api/libraries/{libraryId}/groups/{groupId}/documenttypes";

            return await SendAuthenticatedRequestAsync<ICollection<DmsDocumentTypeItem>>(
                HttpMethod.Get,
                requestUri,
                operationName: $"Get Document Types in library {libraryId} for group {groupId}");
        }

        #endregion


        #region Audit Records

        public async Task AddDocumentUploadedAuditRecordAsync(Guid documentId, Guid? entityId)
        {
            var userDetails = await _currentUserService.GetUserDetailsAsync();

            await _auditService.DataImported(
                entityId,
                null,
                "Document uploaded.",
                userDetails,
                documentId.ToAuditData("Guid"));
        }

        public async Task AddDocumentDownloadedAuditRecordAsync(Guid documentId, Guid? entityId)
        {
            var userDetails = await _currentUserService.GetUserDetailsAsync();

            await _auditService.DataExported(
                entityId,
                null,
                "Document downloaded",
                userDetails,
                documentId.ToAuditData("Guid"));
        }

        public async Task AddDocumentUpdatedAuditRecordAsync(Guid documentId, Guid? entityId)
        {
            var userDetails = await _currentUserService.GetUserDetailsAsync();

            await _auditService.EntitySaved(
                entityId,
                null,
                "Metadata on document updated",
                userDetails,
                documentId.ToAuditData("Guid"));
        }

        public async Task AddDocumentDeletedAuditRecordAsync(Guid libraryId, Guid documentId)
        {
            var userDetails = await _currentUserService.GetUserDetailsAsync();

            var obj = (new
            {
                docid = documentId,
                libid = libraryId
            }).ToAuditData("document");

            await _auditService.EntitySaved(
                documentId,
                "Document",
                "Document marked as deleted",
                userDetails,
                obj);
        }

        #endregion


        #region Metadata Extraction Helpers

        private static string? GetMetadataString(Dictionary<string, object> metadata, string key)
        {
            var kvp = metadata.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            return kvp.Key != null ? kvp.Value?.ToString() : null;
        }

        private static DateTime? GetMetadataDateTime(Dictionary<string, object> metadata, string key)
        {
            var stringValue = GetMetadataString(metadata, key);
            if (DateTime.TryParse(stringValue, out var result))
            {
                return result;
            }
            return null;
        }

        private static DateOnly? GetMetadataDateOnly(Dictionary<string, object> metadata, string key)
        {
            var stringValue = GetMetadataString(metadata, key);
            if (DateOnly.TryParse(stringValue, out var result))
            {
                return result;
            }
            return null;
        }

        private static Guid? GetMetadataGuid(Dictionary<string, object> metadata, string key)
        {
            var stringValue = GetMetadataString(metadata, key);
            if (Guid.TryParse(stringValue, out var result))
            {
                return result;
            }
            return null;
        }

        #endregion
    }
}
