using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using Task = System.Threading.Tasks.Task;

namespace Nexus.DEB.Infrastructure.Services
{
	public class StandardVersionDomainService : DomainServiceBase, IStandardVersionDomainService
	{
		public StandardVersionDomainService(
			ICisService cisService,
			ICbacService cbacService,
			IDebService debService,
			ICurrentUserService currentUserService,
			IDateTimeProvider dateTimeProvider,
			IApplicationSettingsService applicationSettingsService,
			IPawsService pawsService,
			IAuditService auditService,
			ILogger<StandardVersionDomainService> logger) : base(cisService, cbacService, applicationSettingsService, currentUserService, dateTimeProvider, debService, pawsService, auditService, logger, EntityTypes.StandardVersion)
		{
		}

		public async Task<Result<StandardVersion>> CreateStandardVersionAsync(
			Guid ownerId,
			int standardId,
			string versionTitle,
			string delimiter,
			int? majorVersion,
			int? minorVersion,
			DateOnly? effectiveStartDate,
			DateOnly? effectiveEndDate,
			CancellationToken cancellationToken)
		{
			await ValidateFieldsAsync(null, ownerId, versionTitle, effectiveStartDate, effectiveEndDate);

			if (ValidationErrors.Count > 0)
			{
				return Result<StandardVersion>.Failure(ValidationErrors);
			}

			try
			{
				var standard = await this.DebService.GetStandardByIdAsync(standardId, cancellationToken);

				var standardVersion = new StandardVersion()
				{
					EntityTypeTitle = EntityTypes.StandardVersion,
					OwnedById = ownerId,
					SerialNumber = await DebService.GenerateSerialNumberAsync(this.ModuleId, this.InstanceId, EntityTypes.StandardVersion),
					Title = string.Format("{0}{1}{2}",standard.Title, delimiter, versionTitle),
					VersionTitle = versionTitle,
					Delimiter = delimiter,
					MajorVersion = majorVersion,
					MinorVersion = minorVersion,
					EffectiveStartDate = effectiveStartDate,
					EffectiveEndDate = effectiveEndDate,
					Standard = standard,
					StandardId = standard.Id
				};

				standardVersion = await this.DebService.CreateStandardVersionAsync(standardVersion, cancellationToken);

				await this.PawsService.CreateWorkflowInstanceAsync(this.WorkflowId.Value, standardVersion.EntityId, null, null, cancellationToken);

				var fullStandardVersion = await this.DebService.GetStandardVersionByIdAsync(standardVersion.EntityId, cancellationToken);

				return Result<StandardVersion>.Success(fullStandardVersion);
			}
			catch (Exception ex)
			{
				return Result<StandardVersion>.Failure($"An error occurred creating the Standard Version: {ex.Message}");
			}
		}

		public async Task<Result<StandardVersion>> UpdateStandardVersionAsync(
			Guid id,
			Guid ownerId,
			string versionTitle,
			string delimiter,
			int? majorVersion,
			int? minorVersion,
			DateOnly? effectiveStartDate,
			DateOnly? effectiveEndDate,
			CancellationToken cancellationToken)
		{
			var standardVersion = await DebService.GetStandardVersionByIdAsync(id, cancellationToken);

			if (standardVersion == null)
			{
				return Result<StandardVersion>.Failure(new ValidationError()
				{
					Code = "INVALID_STANDARD_VERSION_ID",
					Field = nameof(id),
					Message = "Standard Version does not exist"
				});
			}

			await ValidateFieldsAsync(standardVersion, ownerId, versionTitle, effectiveStartDate, effectiveEndDate);

			if (ValidationErrors.Count > 0)
			{
				return Result<StandardVersion>.Failure(ValidationErrors);
			}

			var standard = await this.DebService.GetStandardByIdAsync(standardVersion.StandardId, cancellationToken);

			standardVersion.OwnedById = ownerId;
			standardVersion.Title = string.Format("{0}{1}{2}", standard.Title, delimiter, versionTitle);
			standardVersion.VersionTitle = versionTitle;
			standardVersion.Delimiter = delimiter;
			standardVersion.MajorVersion = majorVersion;
			standardVersion.MinorVersion = minorVersion;
			standardVersion.EffectiveStartDate = effectiveStartDate;
			standardVersion.EffectiveEndDate = effectiveEndDate;

			try
			{
				await this.DebService.UpdateStandardVersionAsync(standardVersion, cancellationToken);

				var fullStandardVersion = await this.DebService.GetStandardVersionByIdAsync(standardVersion.EntityId, cancellationToken);

				return Result<StandardVersion>.Success(fullStandardVersion);
			}
			catch (Exception ex)
			{
				return Result<StandardVersion>.Failure($"An error occurred updating the Standard Version: {ex.Message}");
			}
		}

		private async Task ValidateFieldsAsync(
			StandardVersion? standardVersion,
			Guid ownerId,
			string versionTitle,
			DateOnly? effectiveStartDate,
			DateOnly? effectiveEndDate)
		{
			await ValidateOwnerAsync(ownerId);

			ValidateStandard(standardVersion);

			ValidateVersionTitle(versionTitle);

			ValidateEffectiveDates(effectiveStartDate, effectiveEndDate);
		}

		protected void ValidateStandard(StandardVersion? standardVersion)
		{
			if (standardVersion != null && standardVersion.StandardId < 1)
			{
				ValidationErrors.Add(
					new ValidationError()
					{
						Code = "INVALID_STANDARD",
						Field = nameof(standardVersion.StandardId),
						Message = "The specified 'standard' was not found."
					});
			}
		}

		protected void ValidateVersionTitle(string versionTitle)
		{
			if (string.IsNullOrWhiteSpace(versionTitle))
			{
				ValidationErrors.Add(
					new ValidationError()
					{
						Code = "INVALID_VERSIONTITLE",
						Field = nameof(versionTitle),
						Message = "The 'version title' is empty."
					});
			}
		}

		protected void ValidateEffectiveDates(DateOnly? effectiveStartDate, DateOnly? effectiveEndDate)
		{
			//if(effectiveStartDate == DateOnly.MinValue)
			//{
			//	ValidationErrors.Add(
			//		new ValidationError()
			//		{
			//			Code = "INVALID_EFFECTIVESTARTDATE",
			//			Field = nameof(effectiveStartDate),
			//			Message = "The 'effective start date' must be provided."
			//		});
			//}

			if (effectiveEndDate.HasValue && effectiveStartDate.HasValue && effectiveEndDate.Value <= effectiveStartDate)
			{
				ValidationErrors.Add(
					new ValidationError()
					{
						Code = "INVALID_EFFECTIVEENDDATE",
						Field = $"{nameof(effectiveStartDate)}, {nameof(effectiveEndDate)}",
						Message = "The 'effective end date' must be later than the 'effective start date'."
					});
			}
		}
	}
}
