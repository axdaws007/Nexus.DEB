using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;
using Nexus.DEB.Domain.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Nexus.DEB.Domain.DebHelper.MyWork.FilterTypes;
using Task = System.Threading.Tasks.Task;

namespace Nexus.DEB.Infrastructure.Services
{
	public class RequirementDomainService : DomainServiceBase, IRequirementDomainService
	{
		public RequirementDomainService(
			ICisService cisService,
			ICbacService cbacService,
			IDebService debService,
			ICurrentUserService currentUserService,
			IDateTimeProvider dateTimeProvider,
			IApplicationSettingsService applicationSettingsService,
			IPawsService pawsService,
			IAuditService auditService,
			ILogger<RequirementDomainService> logger) : base(cisService, cbacService, applicationSettingsService, currentUserService, dateTimeProvider, debService, pawsService, auditService, logger, EntityTypes.Requirement)
		{
		}

		public async Task<Result<RequirementDetail>> CreateRequirementAsync(
            Guid ownerId,
            string serialNumber,
            string title,
            string description,
            DateOnly effectiveStartDate,
            DateOnly effectiveEndDate,
            bool displayTitle,
            bool displayReference,
            short? requirementCategoryId,
            short? requirementTypeId,
            int? complianceWeighting,
            CancellationToken cancellationToken)
		{
			await ValidateFieldsAsync(null, ownerId, serialNumber, title, description );

			if (ValidationErrors.Count > 0)
			{
				return Result<RequirementDetail>.Failure(ValidationErrors);
			}

			try
			{
				var requirement = new Requirement()
				{
					EntityTypeTitle = EntityTypes.Requirement,
					OwnedById = ownerId,
					SerialNumber = serialNumber,
					Description = description,
					Title = title,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate,
                    IsTitleDisplayed = displayTitle,
                    IsReferenceDisplayed = displayReference,
                    RequirementCategoryId = requirementCategoryId,
                    RequirementTypeId = requirementTypeId,
                    ComplianceWeighting = complianceWeighting
				};

				requirement = await this.DebService.CreateRequirementAsync(requirement, cancellationToken);

				await this.PawsService.CreateWorkflowInstanceAsync(this.WorkflowId.Value, requirement.EntityId, null, null, cancellationToken);

                var requirementDetail = await this.DebService.GetRequirementDetailByIdAsync(requirement.EntityId, cancellationToken);

                return Result<RequirementDetail>.Success(requirementDetail);

			}
			catch (Exception ex)
			{
				return Result<RequirementDetail>.Failure($"An error occurred creating the Requirement: {ex.Message}");
			}
		}

		public async Task<Result<RequirementDetail>> UpdateRequirementAsync(
			Guid id,
			Guid ownerId,
            string serialNumber,
            string title,
            string description,
            DateOnly effectiveStartDate,
            DateOnly effectiveEndDate,
            bool displayTitle,
            bool displayReference,
            short? requirementCategoryId,
            short? requirementTypeId,
            int? complianceWeighting,
            CancellationToken cancellationToken)
		{
			var requirement = await DebService.GetRequirementByIdAsync(id, cancellationToken);

			if (requirement == null)
			{
				return Result<RequirementDetail>.Failure(new ValidationError()
				{
					Code = "INVALID_REQUIREMENT_ID",
					Field = nameof(id),
					Message = "Requirement does not exist"
                });
			}

			await ValidateFieldsAsync(requirement, ownerId, serialNumber, title, description);

			if (ValidationErrors.Count > 0)
			{
				return Result<RequirementDetail>.Failure(ValidationErrors);
			}

			requirement.OwnedById = ownerId;
			requirement.SerialNumber = serialNumber;
			requirement.Description = description;
			requirement.Title = title;
			requirement.EffectiveStartDate = effectiveStartDate;
			requirement.EffectiveEndDate = effectiveEndDate;
			requirement.IsTitleDisplayed = displayTitle;
			requirement.IsReferenceDisplayed = displayReference;
			requirement.RequirementCategoryId = requirementCategoryId;
			requirement.RequirementTypeId = requirementTypeId;
			requirement.ComplianceWeighting = complianceWeighting;

            try
			{
				await this.DebService.UpdateRequirementAsync(requirement, cancellationToken);

                var requirementDetail = await this.DebService.GetRequirementDetailByIdAsync(requirement.EntityId, cancellationToken);


                return Result<RequirementDetail>.Success(requirementDetail);
			}
			catch (Exception ex)
			{
				return Result<RequirementDetail>.Failure($"An error occurred updating the Requirement: {ex.Message}");
			}
		}

        private async Task ValidateFieldsAsync(
			Requirement? requirement,
			Guid ownerId,
			string serialNumber,
			string title,
			string description)
		{
			await ValidateOwnerAsync(ownerId);

			// Validate title
			ValidateTitle(title);

            // Validate Serial Number
			ValidateString(serialNumber, nameof(serialNumber));

            //validate description
            ValidateString(description, nameof(description));

        }
	}
}