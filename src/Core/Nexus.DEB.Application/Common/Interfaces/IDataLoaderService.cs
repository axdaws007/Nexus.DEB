using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Application.Common.Interfaces
{
	public interface IDataLoaderService
	{
		Task<IReadOnlyDictionary<Guid, bool>> HasOtherDraftStandardVersionsForStandardsAsync(IEnumerable<Guid> entityIds, CancellationToken cancellationToken = default);

		Task<IReadOnlyDictionary<Guid, string?>> GetWorkflowPseudoStateTitleForEntitiesAsync(List<Guid> entityIds, CancellationToken cancellationToken = default);
	}
}
