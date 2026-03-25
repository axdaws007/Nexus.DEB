using HotChocolate.Authorization;
using HotChocolate.Language;
using Nexus.DEB.Api.Restful;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Dms;
using Nexus.DEB.Application.Common.Models.Events;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Interfaces;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Application.Common.Models;
using System.Text.Json;
using Nexus.DEB.Domain.Models.Other;

namespace Nexus.DEB.Api.GraphQL
{
	[MutationType]
	public static class StandardVersionMutations
	{
		[Authorize(Policy = DebHelper.Policies.CanEditStdVersion)]
		public static async Task<StandardVersion> CreateStandardVersionAsync(
			Guid ownerId,
			short standardId,
			string versionTitle,
			string delimiter,
			int? majorVersion,
			int? minorVersion,
			DateOnly? effectiveStartDate,
			DateOnly? effectiveEndDate,
			IStandardVersionDomainService standardVersionService,
			IDomainEventPublisher eventPublisher,
			CancellationToken cancellationToken = default)
		{
			var result = await standardVersionService.CreateStandardVersionAsync(
				ownerId,
				standardId,
				versionTitle,
				delimiter,
				majorVersion,
				minorVersion,
				effectiveStartDate,
				effectiveEndDate,
				cancellationToken);

			if (!result.IsSuccess)
			{
				throw ExceptionHelper.BuildException(result);
			}

			var standardVersion = result.Data!;

			await eventPublisher.PublishAsync(new EntitySavedEvent
			{
				Entity = standardVersion,
				EntityType = standardVersion.EntityTypeTitle,
				EntityId = standardVersion.EntityId,
				SerialNumber = standardVersion.SerialNumber ?? string.Empty,
				IsNew = true,
			}, cancellationToken);

			return result.Data;
		}

		[Authorize(Policy = DebHelper.Policies.CanEditStdVersion)]
		public static async Task<StandardVersion?> UpdateStandardVersionAsync(
			Guid id,
			Guid ownerId,
			int standardId,
			string versionTitle,
			string delimiter,
			int? majorVersion,
			int? minorVersion,
			DateOnly? effectiveStartDate,
			DateOnly? effectiveEndDate,
			IStandardVersionDomainService standardVersionService,
			IDomainEventPublisher eventPublisher,
			CancellationToken cancellationToken = default)
		{
			var result = await standardVersionService.UpdateStandardVersionAsync(
				id,
				ownerId,
				versionTitle,
				delimiter,
				majorVersion,
				minorVersion,
				effectiveStartDate,
				effectiveEndDate,
				cancellationToken);

			if (!result.IsSuccess)
			{
				throw ExceptionHelper.BuildException(result);
			}

			var standardVersion = result.Data!;

			await eventPublisher.PublishAsync(new EntitySavedEvent
			{
				Entity = standardVersion,
				EntityType = standardVersion.EntityTypeTitle,
				EntityId = standardVersion.EntityId,
				SerialNumber = standardVersion.SerialNumber ?? string.Empty,
				IsNew = false,
			}, cancellationToken);

			return result.Data;
		}

		[Authorize(Policy = DebHelper.Policies.CanUpVersionStdVersion)]
		public static async Task<StandardVersion?> UpVersionStandardVersionAsync(
			Guid upVersionSourceEntityId,
			Guid ownerId,
			short standardId,
			string versionTitle,
			string delimiter,
			int? majorVersion,
			int? minorVersion,
			DateOnly? effectiveStartDate,
			DateOnly? effectiveEndDate,
			bool cloneSections,
			bool cloneRequirementLinks,
			bool cloneCommonEvidence,
			IStandardVersionDomainService standardVersionService,
			IDomainEventPublisher eventPublisher,
			CancellationToken cancellationToken = default)
		{
			var result = await standardVersionService.UpVersionStandardVersionAsync(
				upVersionSourceEntityId,
				ownerId,
				standardId,
				versionTitle,
				delimiter,
				majorVersion,
				minorVersion,
				effectiveStartDate,
				effectiveEndDate,
				cloneSections,
				cloneRequirementLinks,
				cloneCommonEvidence,
				cancellationToken);

			if (!result.IsSuccess)
			{
				throw ExceptionHelper.BuildException(result);
			}

			var standardVersion = result.Data!;

			await eventPublisher.PublishAsync(new EntitySavedEvent
			{
				Entity = standardVersion,
				EntityType = standardVersion.EntityTypeTitle,
				EntityId = standardVersion.EntityId,
				SerialNumber = standardVersion.SerialNumber ?? string.Empty,
				IsNew = true,
			}, cancellationToken);

			return result.Data;
		}

		[Authorize(Policy = DebHelper.Policies.CanEditCommonEvidence)]
		public static async Task<BasketResponse<Guid>> UpdateCommonEvidenceStandardVersionLinksAsync(
			Guid entityId,
			List<Guid>? toDelete,
			List<Guid>? toInsert,
			IApplicationSettingsService applicationSettingsService,
			IDmsService dmsService,
			IDebService debService,
			CancellationToken cancellationToken)
		{
			var libraryId = applicationSettingsService.GetLibraryId(DebHelper.Dms.Libraries.CommonDocuments);
			var svReferences = debService.GetStandardVersions().ToList();

			var isSuccess = true;
			var message = "";

			if (toDelete != null && toDelete.Count > 0)
			{
				foreach (var docId in toDelete)
				{
					var docResponse = await UpdateCommonEvidenceStandardVersionMetaData(libraryId, docId, entityId, false, svReferences, dmsService, debService);
					if (!docResponse.Saved) isSuccess = false;
					else message += $"{docResponse.Title}, ";
				}
			}

			if (toInsert != null && toInsert.Count > 0)
			{
				foreach (var docId in toInsert)
				{
                    var docResponse = await UpdateCommonEvidenceStandardVersionMetaData(libraryId, docId, entityId, true, svReferences, dmsService, debService);
                    if (!docResponse.Saved) isSuccess = false;
                    else message += $"{docResponse.Title}, ";
                }
			}

			if (!isSuccess)
			{
                message = "The following Documents were not updated: " + message.TrimEnd(' ', ',');
            }

			var svCommonEvidenceIds = await debService.GetCommonDocumentIdsForStandardVersionAsync(entityId, applicationSettingsService, dmsService, cancellationToken);

			var result = new BasketResponse<Guid>()
			{
				BasketIds = svCommonEvidenceIds.ToList(),
				IsSuccess = isSuccess,
				Message = message
			};

			return result;
        }

		private static async Task<DmsDocumentResponse> UpdateCommonEvidenceStandardVersionMetaData(
			Guid libraryId,
			Guid documentId, 
			Guid standardVersionid, 
			bool isAdd, 
			List<StandardVersion> svReferences,
			IDmsService dmsService,
            IDebService debService
		)
		{
            var document = await dmsService.GetDocumentAsync(libraryId, documentId);
            var metadata = document.Metadata;

            if (metadata.TryGetValue("StandardVersionIds", out JsonElement standardVersionIdsElement)
				 && standardVersionIdsElement.ValueKind == JsonValueKind.String)
            {
                var standardVersionIdArray = standardVersionIdsElement
                    .GetString()!
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList();

                if (isAdd)
                    standardVersionIdArray.Push(standardVersionid.ToString());
                else
                    standardVersionIdArray.RemoveAll(x => string.Equals(x, standardVersionid.ToString(), StringComparison.OrdinalIgnoreCase));

                // Build metadataLookups array for purpose of DMS history
                List<DmsMetadataLookupItem> metaDataLookupItems = new List<DmsMetadataLookupItem>();
                foreach (var standardVersionId in standardVersionIdArray)
                {
                    metaDataLookupItems.Push(new DmsMetadataLookupItem()
                    {
                        Id = new Guid(standardVersionId),
                        Title = svReferences.Find(sv => sv.EntityId == new Guid(standardVersionId)).Title,
						Type = "standardVersionId"
                    });
                }

                // Update metadata with new comma-separated string
                metadata["StandardVersionIds"] = JsonSerializer.SerializeToElement(standardVersionIdArray);

                var metadataObj = DmsEndpoints.ParseMetadata(JsonSerializer.Serialize(metadata, new JsonSerializerOptions
                {
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
                }));


                DmsDocumentResponse updateResponse = await dmsService.UpdateDocumentAsync(libraryId, documentId, null, metaDataLookupItems, metadataObj);

                return updateResponse;
            }
			else
			{
				throw new Exception("BLAH");
			}
        }
	}
}
