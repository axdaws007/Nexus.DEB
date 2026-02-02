using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			IAuditService auditService) : base(cisService, cbacService, applicationSettingsService, currentUserService, dateTimeProvider, debService, pawsService, auditService, EntityTypes.StandardVersion)
		{
		}

		public async Task<Result<StandardVersion>> CreateStandardVersionAsync(
			Guid ownerId,
			int standardId,
			string versionTitle,
			string delimiter,
			int? majorVersion,
			int? minorVersion,
			DateOnly effectiveStartDate,
			DateOnly? effectiveEndDate,
			CancellationToken cancellationToken)
		{
			//await ValidateFieldsAsync(null, ownerId, title, statementText, reviewDate, requirementScopeCombinations);

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
	}
}
