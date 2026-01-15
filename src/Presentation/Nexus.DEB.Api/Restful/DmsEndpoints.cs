using Microsoft.AspNetCore.Mvc;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Dms;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Infrastructure.Services;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.Json;
using HotChocolate.Execution;

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

            dmsGroup.MapPost("/libraries/{library}/document", AddDocument)
                .RequireAuthorization(policyNames: [DebHelper.Policies.CanAddSoCEvidence])
                .WithName("AddDocument")
                .WithSummary("Add a new document to the library")
                .DisableAntiforgery() // Required for file uploads
                .Produces<DmsDocumentResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            dmsGroup.MapPost("/libraries/{library}/document/{documentId:guid}", UpdateDocument)
                .RequireAuthorization(policyNames: [DebHelper.Policies.CanEditSoCEvidence])
                .WithName("UpdateDocument")
                .WithSummary("Update an existing document")
                .DisableAntiforgery() // Required for file uploads
                .Produces<DmsDocumentResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError);

            dmsGroup.MapGet("/libraries/{library}/document/{documentId:guid}/file", GetDocumentFile)
                .RequireAuthorization(policyNames: [DebHelper.Policies.CanViewSoCEvidence])
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

        private static async Task<IResult> AddDocument(
            [FromRoute] string library,
            [FromForm] IFormFile file,
            [FromForm] string entityId,
            [FromForm] string? title,
            [FromForm] string? description,
            [FromForm] string? author,
            [FromForm] string? documentType,
            [FromServices] IApplicationSettingsService applicationSettingsService,
            [FromServices] IDmsService dmsService,
            [FromServices] IDebService debService,
			[FromServices] IAuditService auditService,
            [FromServices] ICurrentUserService currentUserService)
        {
            try
            {
                DebHelper.Dms.Libraries.Validator.ValidateOrThrow(library);

                if (string.IsNullOrEmpty(documentType)) 
                    documentType = DebHelper.Dms.DocumentTypes.Document;

                var libraryId = applicationSettingsService.GetLibraryId(library);

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
                if (!DebHelper.Dms.DocumentTypes.Validator.IsValid(documentType))
                {
                    return Results.BadRequest(new
                    {
                        error = $"documentType must be '{DebHelper.Dms.DocumentTypes.Document}' or '{DebHelper.Dms.DocumentTypes.Note}'"
                    });
                }

                // Build metadata object matching legacy API expectations
                var metadata = new DmsDocumentMetadata
                {
                    EntityId = parsedEntityId,
                    Title = title,
                    Description = description,
                    Author = author,
                    DocumentType = documentType
                };

                var result = await dmsService.AddDocumentAsync(libraryId, file, metadata);

                if (result == null)
                {
                    return Results.Problem(
                        detail: "Failed to add document. The service returned no data.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }

                if (result.DocumentId.HasValue)
                {
                    await dmsService.AddDocumentAddedAuditRecordAsync(result.DocumentId.Value, new Guid(entityId));
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
        private static async Task<IResult> UpdateDocument(
            [FromRoute] string library,
            [FromRoute] Guid documentId,
            [FromForm] IFormFile? file,
            [FromForm] string? title,
            [FromForm] string? description,
            [FromForm] string? author,
            [FromForm] string? documentType,
            [FromServices] IApplicationSettingsService applicationSettingsService,
            [FromServices] IDmsService dmsService)
        {
            try
            {
                DebHelper.Dms.Libraries.Validator.ValidateOrThrow(library);

                var libraryId = applicationSettingsService.GetLibraryId(library);

                // Validate documentType if provided
                if (!string.IsNullOrWhiteSpace(documentType))
                {
                    var validDocTypes = new[] { "document", "note" };
                    if (!validDocTypes.Contains(documentType.ToLower()))
                    {
                        return Results.BadRequest(new
                        {
                            error = "documentType must be 'document' or 'note'"
                        });
                    }
                }

                // Build metadata - for updates, entityId comes from existing document
                // so it's not required in the update request
                var metadata = new DmsDocumentMetadata
                {
                    EntityId = Guid.Empty, // Will be populated from existing document by legacy API
                    Title = title,
                    Description = description,
                    Author = author,
                    DocumentType = documentType?.ToLower() ?? "document"
                };

                var result = await dmsService.UpdateDocumentAsync(libraryId, documentId, file, metadata);

                if (result == null)
                {
                    return Results.Problem(
                        detail: "Failed to update document. The service returned no data.",
                        statusCode: StatusCodes.Status500InternalServerError);
				}

				if (result.DocumentId.HasValue)
				{
                    var dmsDocument = await dmsService.GetDocumentAsync(libraryId, documentId);

                    if (dmsDocument != null && dmsDocument.EntityId.HasValue)
                    {
                        await dmsService.AddDocumentUpdatedAuditRecordAsync(result.DocumentId.Value, dmsDocument.EntityId.Value);
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
                DebHelper.Dms.Libraries.Validator.ValidateOrThrow(library);

                var libraryId = applicationSettingsService.GetLibraryId(library);

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