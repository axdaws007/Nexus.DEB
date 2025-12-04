using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Application.Common.Interfaces
{
	public interface ISavedSearchDomainService
	{
		Task<Result<SavedSearch>> CreateSavedSearchAsync(string context, string name, string filter, CancellationToken cancellationToken);
	}
}
