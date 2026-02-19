using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
	public interface IRequirementDomainService
	{
		Task<Result<RequirementDetail>> CreateRequirementAsync(
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
			CancellationToken cancellationToken);

		Task<Result<RequirementDetail>> UpdateRequirementAsync(
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
			CancellationToken cancellationToken);
	}
}
