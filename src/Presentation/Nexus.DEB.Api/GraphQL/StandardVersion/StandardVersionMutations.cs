using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Events;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
	[MutationType]
	public static class StandardVersionMutations
	{
		[Authorize(Policy = DebHelper.Policies.CanEditStdVersion)]
		public static async Task<StandardVersion> CreateStandardVersionAsync(
			Guid ownerId,
			int standardId,
			string versionTitle,
			string delimiter,
			int? majorVersion,
			int? minorVersion,
			DateOnly effectiveStartDate,
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
			DateOnly effectiveStartDate,
			DateOnly? effectiveEndDate,
			IStandardVersionDomainService standardVersionService,
			IDomainEventPublisher eventPublisher,
			CancellationToken cancellationToken = default)
		{
			var result = await standardVersionService.UpdateStandardVersionAsync(
				id,
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
	}
}
