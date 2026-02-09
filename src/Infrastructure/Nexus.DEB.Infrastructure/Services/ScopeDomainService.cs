using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;

namespace Nexus.DEB.Infrastructure.Services
{
	public class ScopeDomainService : DomainServiceBase, IScopeDomainService
	{
		public ScopeDomainService(
			ICisService cisService,
			ICbacService cbacService,
			IDebService debService,
			ICurrentUserService currentUserService,
			IDateTimeProvider dateTimeProvider,
			IApplicationSettingsService applicationSettingsService,
			IPawsService pawsService,
			IAuditService auditService,
			ILogger<ScopeDomainService> logger) : base(cisService, cbacService, applicationSettingsService, currentUserService, dateTimeProvider, debService, pawsService, auditService, logger, EntityTypes.Scope)
		{
		}

		public async Task<Result<Scope>> CreateScopeAsync(
			Guid ownerId,
			string title,
			string? description,
			DateOnly? targetImplementationDate,
			CancellationToken cancellationToken)
		{
			await ValidateFieldsAsync(null, ownerId, title);

			if (ValidationErrors.Count > 0)
			{
				return Result<Scope>.Failure(ValidationErrors);
			}

			try
			{
				var scope = new Scope()
				{
					EntityTypeTitle = EntityTypes.Scope,
					OwnedById = ownerId,
					SerialNumber = await DebService.GenerateSerialNumberAsync(this.ModuleId, this.InstanceId, EntityTypes.Scope),
					Description = description,
					Title = title,
					TargetImplementationDate = targetImplementationDate,
				};

				scope = await this.DebService.CreateScopeAsync(scope, cancellationToken);

				await this.PawsService.CreateWorkflowInstanceAsync(this.WorkflowId.Value, scope.EntityId, null, null, cancellationToken);

				return Result<Scope>.Success(scope);
			}
			catch (Exception ex)
			{
				return Result<Scope>.Failure($"An error occurred creating the Scope: {ex.Message}");
			}
		}

		public async Task<Result<Scope>> UpdateScopeAsync(
			Guid id,
			Guid ownerId,
			string title,
			string? description,
			DateOnly? targetImplementationDate,
			CancellationToken cancellationToken)
		{
			var scope = await DebService.GetScopeByIdAsync(id, cancellationToken);

			if (scope == null)
			{
				return Result<Scope>.Failure(new ValidationError()
				{
					Code = "INVALID_SCOPE_ID",
					Field = nameof(id),
					Message = "Scope does not exist"
				});
			}

			await ValidateFieldsAsync(scope, ownerId, title);

			if (ValidationErrors.Count > 0)
			{
				return Result<Scope>.Failure(ValidationErrors);
			}

			scope.OwnedById = ownerId;
			scope.Title = title;
			scope.Description = description;
			scope.TargetImplementationDate = targetImplementationDate;

			try
			{
				await this.DebService.UpdateScopeAsync(scope, cancellationToken);

				return Result<Scope>.Success(scope);
			}
			catch (Exception ex)
			{
				return Result<Scope>.Failure($"An error occurred updating the Scope: {ex.Message}");
			}
		}

		public async Task<Result<ScopeDetail?>> UpdateScopeRequirementsAsync(
			Guid scopeId,
			Guid standardVersionId,
			List<Guid> idsToAdd,
			List<Guid> idsToRemove,
			bool addAll,
			bool removeAll,
			CancellationToken cancellationToken)
		{
			var scope = await DebService.GetScopeByIdAsync(scopeId, cancellationToken);

			if (scope == null)
			{
				return Result<ScopeDetail?>.Failure(new ValidationError()
				{
					Code = "INVALID_SCOPE_ID",
					Field = nameof(scopeId),
					Message = "Scope does not exist"
				});
			}

			var standardVersion = await DebService.GetStandardVersionByIdAsync(standardVersionId, cancellationToken);

			if (standardVersion == null)
			{
				return Result<ScopeDetail?>.Failure(new ValidationError()
				{
					Code = "INVALID_STANDARDVERSION_ID",
					Field = nameof(standardVersionId),
					Message = "Standard Version does not exist"
				});
			}

			try
			{
				var scopeDetail = await DebService.UpdateScopeRequirementsAsync(
					scopeId,
					standardVersion,
					idsToAdd,
					idsToRemove,
					addAll,
					removeAll,
					cancellationToken);

				return Result<ScopeDetail?>.Success(scopeDetail);
			}
			catch (Exception ex)
			{
				return Result<ScopeDetail?>.Failure($"An error occurred updating the Scope Requirements: {ex.Message}");
			}
		}

		private async Task ValidateFieldsAsync(
			Scope? scope,
			Guid ownerId,
			string title)
		{
			await ValidateOwnerAsync(ownerId);

			// Validate title
			ValidateTitle(title);
		}
	}
}