using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class ScopeQueries
    {
        [Authorize]
        [UseOffsetPaging]
        [UseSorting]
        public static IQueryable<ScopeSummary> GetScopesForGrid(
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetScopesForGrid();

        [Authorize]
        [UseSorting]
        public static IQueryable<Scope> GetScopes(
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetScopes();

        [Authorize]
        public async static Task<ICollection<FilterItemEntity>> GetScopesLookupAsync(
            IDebService debService,
            IResolverContext resolverContext,
            CancellationToken cancellationToken)
            => await debService.GetScopesLookupAsync(cancellationToken);

        [Authorize]
        public static async Task<ICollection<ScopeCondensed>> GetScopesForRequirement(
            Guid requirementId,
            Guid? statementId,
            IDebService debService,
            CancellationToken cancellationToken)
            => await debService.GetScopesForRequirementAsync(requirementId, statementId, cancellationToken);

        [Authorize]
        public static async Task<ScopeDetail> GetScopeById(Guid id, IDebService debService, CancellationToken cancellationToken)
            => await debService.GetScopeById(id, cancellationToken);

		[Authorize]
		public static async Task<ScopeChildCounts> GetChildCountsForScope(
			Guid id,
			IDebService debService,
			IApplicationSettingsService applicationSettingsService,
			IDmsService dmsService,
			CancellationToken cancellationToken)
		{
			var counts = await debService.GetChildCountsForScopeAsync(id, cancellationToken);

			var debLibraryId = applicationSettingsService.GetLibraryId(DebHelper.Dms.Libraries.DebDocuments);
			var commonLibraryId = applicationSettingsService.GetLibraryId(DebHelper.Dms.Libraries.CommonDocuments);

			var debDocumentCount = await dmsService.GetEntityDocumentCountAsync(debLibraryId, id);
			var commonDocumentCount = await dmsService.GetEntityDocumentCountAsync(commonLibraryId, id);

			counts.AttachmentsCount = debDocumentCount + commonDocumentCount;

			return counts;
		}
	}
}
