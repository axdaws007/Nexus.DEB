using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Application.Common.Interfaces
{
	public interface IScopeDomainService
	{
		Task<Result<Scope>> CreateScopeAsync(
			Guid ownerId,
			string title,
			string? description,
			DateOnly? targetImplementationDate,
			CancellationToken cancellationToken);

		Task<Result<Scope>> UpdateScopeAsync(
			Guid id,
			Guid ownerId,
			string title,
			string? description,
			DateOnly? targetImplementationDate,
			CancellationToken cancellationToken);

		Task<Result<ScopeDetail?>> UpdateScopeRequirementsAsync(
			Guid scopeId,
			Guid standardVersionId,
			List<Guid> idsToAdd,
			List<Guid> idsToRemove,
			bool addAll,
			bool removeAll,
			CancellationToken cancellationToken);
	}
}
