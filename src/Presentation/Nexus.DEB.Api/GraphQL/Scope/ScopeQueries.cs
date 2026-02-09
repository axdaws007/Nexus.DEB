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
        public static async Task<ScopeDetail?> GetScopeById(Guid scopeId, IDebService debService, CancellationToken cancellationToken)
            => await debService.GetScopeDetailByIdAsync(scopeId, cancellationToken);

        [Authorize]
        public static async Task<List<StandardVersionRequirements>> GetStandardVersionRequirementsForScopeAsync(Guid scopeId, IDebService debService, CancellationToken cancellationToken)
			=> await debService.GetStandardVersionRequirementsForScopeAsync(scopeId, cancellationToken);

		[Authorize]
		public static async Task<ScopeChildCounts> GetChildCountsForScope(
			Guid scopeId,
			IDebService debService,
			IApplicationSettingsService applicationSettingsService,
			IDmsService dmsService,
			CancellationToken cancellationToken)
		{
			var counts = await debService.GetChildCountsForScopeAsync(scopeId, cancellationToken);

			var debLibraryId = applicationSettingsService.GetLibraryId(DebHelper.Dms.Libraries.DebDocuments);
			var commonLibraryId = applicationSettingsService.GetLibraryId(DebHelper.Dms.Libraries.CommonDocuments);

			var debDocumentCount = await dmsService.GetEntityDocumentCountAsync(debLibraryId, scopeId);
            //TODO: re-work after linking to common evidence is done.
            //var commonDocumentCount = await dmsService.GetEntityDocumentCountAsync(commonLibraryId, scopeId);
            var commonDocumentCount = 0;

			counts.AttachmentsCount = debDocumentCount + commonDocumentCount;

			return counts;
		}
	}
}
