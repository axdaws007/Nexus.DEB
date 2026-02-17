using Microsoft.AspNetCore.Mvc;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Dms;
using Nexus.DEB.Domain;
using System.Text.Json;

namespace Nexus.DEB.Api.Restful
{
    public static class DmsEndpoints
    {
        private static readonly JsonSerializerOptions MetadataJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static void MapDmsEndpoints(this WebApplication app)
        {
            var dmsGroup = app.MapGroup("/api/dms")
                .WithTags("Document Management")
                .RequireAuthorization()
                .WithOpenApi();

            dmsGroup.MapPost("/libraries/{library}/document", AddDocument)
                .RequireAuthorization(policyNames: [DebHelper.Policies.CanAddDocuments])
                .WithName("AddDocument")
                .WithSummary("Add a new document to the library")
                .DisableAntiforgery() // Required for file uploads
                .Produces<DmsDocumentResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            dmsGroup.MapPost("/libraries/{library}/document/{documentId:guid}", UpdateDocument)
                .RequireAuthorization(policyNames: [DebHelper.Policies.CanEditDocuments])
                .WithName("UpdateDocument")
                .WithSummary("Update an existing document")
                .DisableAntiforgery() // Required for file uploads
                .Produces<DmsDocumentResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            dmsGroup.MapGet("/libraries/{library}/document/{documentId:guid}/file", GetDocumentFile)
                .RequireAuthorization(policyNames: [DebHelper.Policies.CanViewDocuments])
                .WithName("GetDocumentFile")
                .WithSummary("Download a document file")
                .Produces<FileResult>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            //dmsGroup.MapGet("/debug/claims", (HttpContext context) =>
            //{
            //    var claims = context.User.Claims
            //        .Select(c => new { c.Type, c.Value })
            //        .ToList();
            //    return Results.Ok(claims);
            //})
            //.WithName("DebugClaims");
        }

        /// <summary>
        /// Adds a new document to a library.
        /// Accepts multipart/form-data with a file and JSON metadata string.
        /// </summary>
        /// <param name="library">The library name</param>
        /// <param name="file">The file to upload (from form)</param>
        /// <param name="metadata">JSON string containing metadata key-value pairs</param>
        /// <param name="applicationSettingsService">Injected settings service</param>
        /// <param name="dmsService">Injected DMS service</param>
        /// <param name="debService">Injected DEB service</param>
        /// <returns>Document response with metadata</returns>
        private static async Task<IResult> AddDocument(
            [FromRoute] string library,
            [FromForm] IFormFile file,
            [FromForm] string? metadata,
            [FromServices] IApplicationSettingsService applicationSettingsService,
            [FromServices] IDmsService dmsService,
            [FromServices] IDebService debService,
            CancellationToken cancellationToken)
        {
            try
            {
                DebHelper.Dms.Libraries.Validator.ValidateOrThrow(library);

                var dmsSettings = await dmsService.GetSettingsAsync();

                // Validate file
                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(new { error = "File is required" });
                }

                // Validate file size 
                if (file.Length > dmsSettings.MaximumFileSizeInBytes)
                {
                    return Results.BadRequest(new
                    {
                        error = $"File size exceeds maximum allowed size of {dmsSettings.MaximumFileSizeInBytes / 1024 / 1024} MB"
                    });
                }

                if (dmsSettings.AllowedFileExtensions.Count > 0)
                {
                    var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();

                    if (!dmsSettings.AllowedFileExtensions.Contains(fileExtension))
                    {
                        return Results.BadRequest(new
                        {
                            error = $"The file extension '{fileExtension}' is not one of the allowed extensions."
                        });
                    }
                }
                
                // Parse metadata from JSON string
                var metadataObj = ParseMetadata(metadata);
                if (metadataObj == null)
                {
                    return Results.BadRequest(new { error = "metadata must be a valid JSON object with string values" });
                }

                var libraryId = applicationSettingsService.GetLibraryId(library);

                var metadataLookups = await GetLookupsForMetadataAsync(
                    debService, dmsService, metadataObj,
                    library, libraryId, null, cancellationToken);

                var result = await dmsService.AddDocumentAsync(libraryId, file, metadataLookups, metadataObj);

                if (result == null)
                {
                    return Results.Problem(
                        detail: "Failed to add document. The service returned no data.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }


                // Create audit record if entityId was provided and document was created

                if (result.DocumentId.HasValue)
                {
                    if (metadataObj.TryGetGuid("entityId", out var entityId))
                    {
                        await dmsService.AddDocumentUploadedAuditRecordAsync(result.DocumentId.Value, entityId);
                    }
                    else
                    {
                        await dmsService.AddDocumentUploadedAuditRecordAsync(result.DocumentId.Value, null);
                    }
                }

				return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (FormatException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Updates an existing document in a library.
        /// Accepts multipart/form-data with an optional file and JSON metadata string.
        /// </summary>
        /// <param name="library">The library name</param>
        /// <param name="documentId">The document ID to update</param>
        /// <param name="file">The new file to upload (optional)</param>
        /// <param name="metadata">JSON string containing metadata key-value pairs</param>
        /// <param name="applicationSettingsService">Injected settings service</param>
        /// <param name="dmsService">Injected DMS service</param>
        /// <returns>Document response with updated metadata</returns>
        private static async Task<IResult> UpdateDocument(
            [FromRoute] string library,
            [FromRoute] Guid documentId,
            [FromForm] IFormFile? file,
            [FromForm] string? metadata,
            [FromServices] IApplicationSettingsService applicationSettingsService,
            [FromServices] IDebService debService,
            [FromServices] IDmsService dmsService,
            CancellationToken cancellationToken)
        {
            try
            {
                DebHelper.Dms.Libraries.Validator.ValidateOrThrow(library);

                var dmsSettings = await dmsService.GetSettingsAsync();

                // Validate file
                if (file != null && file.Length > 0)
                {
                    // Validate file size 
                    if (file.Length > dmsSettings.MaximumFileSizeInBytes)
                    {
                        return Results.BadRequest(new
                        {
                            error = $"File size exceeds maximum allowed size of {dmsSettings.MaximumFileSizeInBytes / 1024 / 1024} MB"
                        });
                    }

                    if (dmsSettings.AllowedFileExtensions.Count > 0)
                    {
                        var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();

                        if (!dmsSettings.AllowedFileExtensions.Contains(fileExtension))
                        {
                            return Results.BadRequest(new
                            {
                                error = $"The file extension '{fileExtension}' is not one of the allowed extensions."
                            });
                        }
                    }
                }

                // Parse metadata from JSON string
                var metadataObj = ParseMetadata(metadata);
                if (metadataObj == null)
                {
                    return Results.BadRequest(new { error = "metadata must be a valid JSON object with string values" });
                }

                var libraryId = applicationSettingsService.GetLibraryId(library);

                var metadataLookups = await GetLookupsForMetadataAsync(
                    debService, dmsService, metadataObj,
                    library, libraryId, documentId, cancellationToken);

                var result = await dmsService.UpdateDocumentAsync(libraryId, documentId, file, metadataLookups, metadataObj);

                if (result == null)
                {
                    return Results.Problem(
                        detail: "Failed to update document. The service returned no data.",
                        statusCode: StatusCodes.Status500InternalServerError);
				}

				if (result.DocumentId.HasValue)
				{
                    var dmsDocument = await dmsService.GetDocumentAsync(libraryId, documentId);

                    if (dmsDocument != null && dmsDocument.Metadata != null
                        && dmsDocument.Metadata.ContainsKey("EntityId"))
                    {
                        var hasEntityId = Guid.TryParse(dmsDocument.Metadata["EntityId"].GetString(), out var entityId);

                        if (hasEntityId && entityId != Guid.Empty)
                        {
                            if (file != null)
                            {
                                await dmsService.AddDocumentUploadedAuditRecordAsync(result.DocumentId.Value, entityId);
                            }
                            else
                            {
                                await dmsService.AddDocumentUpdatedAuditRecordAsync(result.DocumentId.Value, entityId);
                            }
                        }
                    }
				}

				return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (FormatException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task<List<DmsMetadataLookupItem>> GetLookupsForMetadataAsync(
            IDebService debService,
            IDmsService dmsService,
            DmsDocumentMetadata metadataObj,
            string library,
            Guid libraryId,
            Guid? documentId,
            CancellationToken cancellationToken)
        {
            var lookupRequests = new List<(string Type, Guid Id)>();

            // Collect new IDs from the incoming metadata
            if (metadataObj.TryGetGuid("entityId", out var entityId))
            {
                lookupRequests.Add(("entityId", entityId));
            }

            var standardVersionIds = metadataObj.GetArrayOrDefault<Guid>(
                    "standardVersionIds",
                    e => Guid.TryParse(e.GetString(), out var g) ? g : (Guid?)null);

            foreach (var svId in standardVersionIds)
            {
                lookupRequests.Add(("standardVersionId", svId));
            }

            // For updates, fetch the current document to get the old IDs
            if (documentId.HasValue)
            {
                if (library == DebHelper.Dms.Libraries.DebDocuments)
                {
                    var currentDoc = await dmsService.GetDebLibraryDocumentAsync(libraryId, documentId.Value);
                    if (currentDoc?.EntityId.HasValue == true && currentDoc.EntityId.Value != Guid.Empty)
                    {
                        lookupRequests.Add(("entityId", currentDoc.EntityId.Value));
                    }
                }
                else if (library == DebHelper.Dms.Libraries.CommonDocuments)
                {
                    var currentDoc = await dmsService.GetCommonLibraryDocumentAsync(libraryId, documentId.Value);
                    if (!string.IsNullOrWhiteSpace(currentDoc?.StandardVersionIds))
                    {
                        var currentSvIds = currentDoc.StandardVersionIds
                            .Split(',')
                            .Select(s => s.Trim())
                            .Where(s => Guid.TryParse(s, out _))
                            .Select(Guid.Parse);

                        foreach (var svId in currentSvIds)
                        {
                            lookupRequests.Add(("standardVersionId", svId));
                        }
                    }
                }
            }

            if (lookupRequests.Count == 0)
            {
                return new List<DmsMetadataLookupItem>();
            }

            // Deduplicate before resolving
            var distinctIds = lookupRequests.Select(r => r.Id).Distinct().ToList();
            var items = await debService.GetEntityHeadsAsync(distinctIds, cancellationToken);

            var lookups = lookupRequests
                .Where(r => items.ContainsKey(r.Id))
                .Select(r => new DmsMetadataLookupItem
                {
                    Type = r.Type,
                    Id = r.Id,
                    Title = string.IsNullOrEmpty(items[r.Id].SerialNumber)
                        ? items[r.Id].Title
                        : items[r.Id].SerialNumber
                })
                .GroupBy(r => new { r.Type, r.Id })
                .Select(g => g.First())
                .ToList();

            return lookups;
        }

        /// <summary>
        /// Downloads a document file from a library.
        /// Returns the file as an attachment with proper headers for browser download.
        /// </summary>
        /// <param name="libraryId">The library ID</param>
        /// <param name="documentId">The document ID to download</param>
        /// <param name="version">Optional version number</param>
        /// <param name="dmsService">Injected DMS service</param>
        /// <returns>File stream with appropriate headers</returns>
        private static async Task<IResult> GetDocumentFile(
            [FromRoute] string library,
            [FromRoute] Guid documentId,
            [FromQuery] int? version,
            [FromServices] IApplicationSettingsService applicationSettingsService,
            [FromServices] IDmsService dmsService)
        {
            try
            {
                Guid? entityId = null;
                DebHelper.Dms.Libraries.Validator.ValidateOrThrow(library);

                var libraryId = applicationSettingsService.GetLibraryId(library);

                var documentFile = await dmsService.GetDocumentFileAsync(libraryId, documentId, version);

                if (library == DebHelper.Dms.Libraries.DebDocuments)
                {
                    var debDocument = await dmsService.GetDebLibraryDocumentAsync(libraryId, documentId, version);

                    if (debDocument != null)
                    {
                        entityId = debDocument.EntityId;
                    }
                }

				if (documentFile == null)
                {
                    return Results.NotFound(new
                    {
                        error = $"Document {documentId} not found in library {libraryId}"
                    });
                }

                await dmsService.AddDocumentDownloadedAuditRecordAsync(documentId, entityId);

                // Return the file with proper headers
                return Results.File(
                    documentFile.FileData,
                    contentType: documentFile.MimeType,
                    fileDownloadName: documentFile.FileName,
                    enableRangeProcessing: true);
            }
            catch (FormatException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        private static DmsDocumentMetadata? ParseMetadata(string? metadata)
        {
            if (string.IsNullOrWhiteSpace(metadata))
            {
                return new DmsDocumentMetadata();
    }

            try
            {
                var fields = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                    metadata,
                    MetadataJsonOptions);

                return new DmsDocumentMetadata
                {
                    RawJson = metadata,
                    Fields = fields ?? new Dictionary<string, JsonElement>()
                };
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}