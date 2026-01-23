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
	public static class ScopeMutations
	{
		[Authorize(Policy = DebHelper.Policies.CanCreateOrEditScope)]
		public static async Task<Scope?> CreateScopeAsync(
			Guid ownerId,
			string title,
			string description,
			DateOnly? targetImplementationDate,
			IScopeDomainService scopeService,
			IDomainEventPublisher eventPublisher,
			CancellationToken cancellationToken = default)
		{
			var result = await scopeService.CreateScopeAsync(
				ownerId,
				title,
				description,
				targetImplementationDate,
				cancellationToken);

			if (!result.IsSuccess)
			{
				throw ExceptionHelper.BuildException(result);
			}

			var scopeDetail = result.Data!;

			await eventPublisher.PublishAsync(new EntitySavedEvent
			{
				Entity = scopeDetail,
				EntityType = scopeDetail.EntityTypeTitle,
				EntityId = scopeDetail.EntityId,
				SerialNumber = scopeDetail.SerialNumber ?? string.Empty,
				IsNew = true,
			}, cancellationToken);

			return result.Data;
		}

		[Authorize(Policy = DebHelper.Policies.CanCreateOrEditScope)]
		public static async Task<Scope?> UpdateScopeAsync(
			Guid id,
			Guid ownerId,
			string title,
			string description,
			DateOnly? targetImplementationDate,
			IScopeDomainService scopeService,
			IDomainEventPublisher eventPublisher,
			CancellationToken cancellationToken = default)
		{
			var result = await scopeService.UpdateScopeAsync(
				id,
				ownerId,
				title,
				description,
				targetImplementationDate,
				cancellationToken);

			if (!result.IsSuccess)
			{
				throw ExceptionHelper.BuildException(result);
			}

			var scopeDetail = result.Data!;

			await eventPublisher.PublishAsync(new EntitySavedEvent
			{
				Entity = scopeDetail,
				EntityType = scopeDetail.EntityTypeTitle,
				EntityId = scopeDetail.EntityId,
				SerialNumber = scopeDetail.SerialNumber ?? string.Empty,
				IsNew = true,
			}, cancellationToken);

			return result.Data;
		}

		[Authorize(Policy = DebHelper.Policies.CanCreateOrEditScope)]
		public static async Task<Result> UpdateScopeRequirementsAsync(Guid scopeId, Guid standardVersionid, List<Guid> idsToAdd, List<Guid> idsToRemove, bool addAll, bool removeAll, IScopeDomainService scopeService, IDomainEventPublisher eventPublisher, CancellationToken cancellationToken)
		{
			var result = await scopeService.UpdateScopeRequirementsAsync(scopeId, standardVersionid, idsToAdd, idsToRemove, addAll, removeAll, cancellationToken);
			
			if (!result.IsSuccess)
			{
				throw ExceptionHelper.BuildException(result);
			}
			
			var scope = result.Data!;
			
			await eventPublisher.PublishAsync(new EntitySavedEvent
			{
				Entity = scope,
				EntityType = scope.EntityTypeTitle,
				EntityId = scope.EntityId,
				SerialNumber = scope.SerialNumber ?? string.Empty,
				IsNew = false,
			}, cancellationToken);
			
			return result;
		}
	}
}
