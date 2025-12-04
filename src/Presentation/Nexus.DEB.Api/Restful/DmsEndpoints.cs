using Microsoft.AspNetCore.Mvc;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Dms;
using Nexus.DEB.Domain;

namespace Nexus.DEB.Api.Restful
{
    public static class DmsEndpoints
    {
        public static void MapDmsEndpoints(this WebApplication app)
        {
            var dmsGroup = app.MapGroup("/api/dms")
                .WithTags("Document Management")
                .RequireAuthorization()
                .WithOpenApi();

            dmsGroup.MapPost("/libraries/deb-documents/document", AddDebDocument)
                .WithName("AddDebDocument")
                .WithSummary("Add a new document to the DEB document library")
                .DisableAntiforgery() // Required for file uploads
                .Produces<DmsDocumentResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            dmsGroup.MapPost("/libraries/common-documents/document", AddCommonDocument)
                .WithName("AddCommonDocument")
                .WithSummary("Add a new document to the Common document library")
                .DisableAntiforgery() // Required for file uploads
                .Produces<DmsDocumentResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            //dmsGroup.MapPost("/libraries/{libraryId:guid}/document/{documentId:guid}", UpdateDocument)
            //    .WithName("UpdateDocument")
            //    .WithSummary("Update an existing document")
            //    .DisableAntiforgery() // Required for file uploads
            //    .Produces<DmsDocumentResponse>(StatusCodes.Status200OK)
            //    .Produces(StatusCodes.Status400BadRequest)
            //    .Produces(StatusCodes.Status401Unauthorized)
            //    .Produces(StatusCodes.Status403Forbidden)
            //    .Produces(StatusCodes.Status500InternalServerError);

            //dmsGroup.MapGet("/libraries/{libraryId:guid}/document/{documentId:guid}/file", GetDocumentFile)
            //    .WithName("GetDocumentFile")
            //    .WithSummary("Download a document file")
            //    .Produces<FileResult>(StatusCodes.Status200OK)
            //    .Produces(StatusCodes.Status400BadRequest)
            //    .Produces(StatusCodes.Status401Unauthorized)
            //    .Produces(StatusCodes.Status404NotFound)
            //    .Produces(StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Adds a document to the DEB Documents library.
        /// </summary>
        private static async Task<IResult> AddDebDocument(
            [FromForm] IFormFile file,
            [FromForm] string entityId,
            [FromForm] string? title,
            [FromForm] string? description,
            [FromForm] string? author,
            [FromForm] string? documentType,
            [FromServices] IApplicationSettingsService applicationSettingsService,
            [FromServices] IDmsService dmsService)
        {
            var libraryId = applicationSettingsService.GetLibraryId(DebHelper.DmsLibraries.DebDocuments);

            return await AddDocument(libraryId, file, entityId, title, description, author, documentType, dmsService);
        }

        /// <summary>
        /// Adds a document to the Common Documents library.
        /// </summary>
        private static async Task<IResult> AddCommonDocument(
            [FromForm] IFormFile file,
            [FromForm] string entityId,
            [FromForm] string? title,
            [FromForm] string? description,
            [FromForm] string? author,
            [FromForm] string? documentType,
            [FromServices] IApplicationSettingsService applicationSettingsService,
            [FromServices] IDmsService dmsService)
        {
            var libraryId = applicationSettingsService.GetLibraryId(DebHelper.DmsLibraries.CommonDocuments);

            return await AddDocument(libraryId, file, entityId, title, description, author, documentType, dmsService);
        }

        /// <summary>
        /// Adds a new document to a library.
        /// Accepts multipart/form-data with a file and individual metadata fields.
        /// </summary>
        /// <param name="libraryId">The library ID</param>
        /// <param name="file">The file to upload (from form)</param>
        /// <param name="entityId">The entity ID this document is associated with (REQUIRED)</param>
        /// <param name="title">Document title (optional)</param>
        /// <param name="description">Document description (optional)</param>
        /// <param name="author">Document author (optional)</param>
        /// <param name="documentType">Document type: "document" or "note" (defaults to "document")</param>
        /// <param name="dmsService">Injected DMS service</param>
        /// <returns>Document response with ID and metadata</returns>
        private static async Task<IResult> AddDocument(
            Guid libraryId,
            IFormFile file,
            string entityId,
            string? title,
            string? description,
            string? author,
            string? documentType,
            IDmsService dmsService)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(new { error = "File is required" });
                }

                // Validate file size (e.g., 50 MB limit)
                const long maxFileSize = 50 * 1024 * 1024; // 50 MB
                if (file.Length > maxFileSize)
                {
                    return Results.BadRequest(new
                    {
                        error = $"File size exceeds maximum allowed size of {maxFileSize / 1024 / 1024} MB"
                    });
                }

                // Validate and parse entityId
                if (string.IsNullOrWhiteSpace(entityId))
                {
                    return Results.BadRequest(new { error = "entityId is required" });
                }

                if (!Guid.TryParse(entityId, out var parsedEntityId))
                {
                    return Results.BadRequest(new { error = "entityId must be a valid GUID" });
                }

                // Validate documentType
                var validDocTypes = new[] { "document", "note" };
                var docType = string.IsNullOrWhiteSpace(documentType)
                    ? "document"
                    : documentType.ToLower();

                if (!validDocTypes.Contains(docType))
                {
                    return Results.BadRequest(new
                    {
                        error = "documentType must be 'document' or 'note'"
                    });
                }

                // Build metadata object matching legacy API expectations
                var metadata = new DmsDocumentMetadata
                {
                    EntityId = parsedEntityId,
                    Title = title,
                    Description = description,
                    Author = author,
                    DocumentType = docType
                };

                var result = await dmsService.AddDocumentAsync(libraryId, file, metadata);

                if (result == null)
                {
                    return Results.Problem(
                        detail: "Failed to add document. The service returned no data.",
                        statusCode: StatusCodes.Status500InternalServerError);
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

        ///// <summary>
        ///// Updates an existing document in a library.
        ///// Accepts multipart/form-data with a file and optional metadata fields.
        ///// </summary>
        ///// <param name="libraryId">The library ID</param>
        ///// <param name="documentId">The document ID to update</param>
        ///// <param name="file">The new file to upload (from form)</param>
        ///// <param name="title">Document title (optional)</param>
        ///// <param name="description">Document description (optional)</param>
        ///// <param name="author">Document author (optional)</param>
        ///// <param name="documentType">Document type: "document" or "note"</param>
        ///// <param name="dmsService">Injected DMS service</param>
        ///// <returns>Document response with updated metadata</returns>
        //private static async Task<IResult> UpdateDocument(
        //    [FromRoute] Guid libraryId,
        //    [FromRoute] Guid documentId,
        //    [FromForm] IFormFile file,
        //    [FromForm] string? title,
        //    [FromForm] string? description,
        //    [FromForm] string? author,
        //    [FromForm] string? documentType,
        //    [FromServices] IDmsService dmsService)
        //{
        //    try
        //    {
        //        // Validate file
        //        if (file == null || file.Length == 0)
        //        {
        //            return Results.BadRequest(new { error = "File is required" });
        //        }

        //        // Validate file size
        //        const long maxFileSize = 50 * 1024 * 1024; // 50 MB
        //        if (file.Length > maxFileSize)
        //        {
        //            return Results.BadRequest(new
        //            {
        //                error = $"File size exceeds maximum allowed size of {maxFileSize / 1024 / 1024} MB"
        //            });
        //        }

        //        // Validate documentType if provided
        //        if (!string.IsNullOrWhiteSpace(documentType))
        //        {
        //            var validDocTypes = new[] { "document", "note" };
        //            if (!validDocTypes.Contains(documentType.ToLower()))
        //            {
        //                return Results.BadRequest(new
        //                {
        //                    error = "documentType must be 'document' or 'note'"
        //                });
        //            }
        //        }

        //        // Build metadata - for updates, entityId comes from existing document
        //        // so it's not required in the update request
        //        var metadata = new DmsDocumentMetadata
        //        {
        //            EntityId = Guid.Empty, // Will be populated from existing document by legacy API
        //            Title = title,
        //            Description = description,
        //            Author = author,
        //            DocumentType = documentType?.ToLower() ?? "document"
        //        };

        //        var result = await dmsService.UpdateDocumentAsync(libraryId, documentId, file, metadata);

        //        if (result == null)
        //        {
        //            return Results.Problem(
        //                detail: "Failed to update document. The service returned no data.",
        //                statusCode: StatusCodes.Status500InternalServerError);
        //        }

        //        return Results.Ok(result);
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return Results.BadRequest(new { error = ex.Message });
        //    }
        //    catch (FormatException ex)
        //    {
        //        return Results.BadRequest(new { error = ex.Message });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return Results.BadRequest(new { error = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Results.Problem(
        //            detail: ex.Message,
        //            statusCode: StatusCodes.Status500InternalServerError);
        //    }
        //}

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
            [FromRoute] Guid libraryId,
            [FromRoute] Guid documentId,
            [FromQuery] int? version,
            [FromServices] IDmsService dmsService)
        {
            try
            {
                var document = await dmsService.GetDocumentFileAsync(libraryId, documentId, version);

                if (document == null)
                {
                    return Results.NotFound(new
                    {
                        error = $"Document {documentId} not found in library {libraryId}"
                    });
                }

                // Return the file with proper headers
                return Results.File(
                    document.FileData,
                    contentType: document.MimeType,
                    fileDownloadName: document.FileName,
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
    }
}