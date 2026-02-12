using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
	public interface IStandardVersionDomainService
	{
		Task<Result<StandardVersion>> CreateStandardVersionAsync(
			Guid ownerId,
			int standardId,
			string versionTitle,
			string delimiter,
			int? majorVersion,
			int? minorVersion,
			DateOnly effectiveStartDate,
			DateOnly? effectiveEndDate,
			CancellationToken cancellationToken);

		Task<Result<StandardVersion>> UpdateStandardVersionAsync(
			Guid id,
			Guid ownerId,
			string versionTitle,
			string delimiter,
			int? majorVersion,
			int? minorVersion,
			DateOnly effectiveStartDate,
			DateOnly? effectiveEndDate,
			CancellationToken cancellationToken);
	}
}
