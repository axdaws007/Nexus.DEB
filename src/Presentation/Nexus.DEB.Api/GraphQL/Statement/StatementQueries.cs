using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class StatementQueries
    {
        [Authorize]
        [UseOffsetPaging]
        [UseSorting]
        public static IQueryable<StatementSummary> GetStatementsForGrid(
            StatementSummaryFilters? filters,
            IDebService debService,
            ICisService cisService,
            IResolverContext resolverContext)
        {
            var f = filters is null
            ? new Application.Common.Models.StatementSummaryFilters()
            : new Application.Common.Models.StatementSummaryFilters
            {
                ModifiedFrom = filters.ModifiedFrom,
                ModifiedTo = filters.ModifiedTo,
                ScopeIds = filters.ScopeIds,
                SearchText = filters.SearchText?.Trim(),
                StandardVersionIds = filters.StandardVersionIds,
                StatusIds = filters.StatusIds,
            };

            if (!string.IsNullOrEmpty(filters?.OwnedBy))
            {
                var posts = cisService.GetPostsBySearchTextAsync(filters.OwnedBy).GetAwaiter().GetResult();

                if (posts != null && posts.Count > 0)
                {
                    f.OwnedByIds = [.. posts.Select(x => x.ID)];
                }
            }

            return debService.GetStatementsForGrid(f);
        }

        [Authorize]
        public static async Task<StatementDetail?> GetStatementById(
            Guid statementId,
            IDebService debService,
            IResolverContext resolverContext,
            CancellationToken cancellationToken)
            => await debService.GetStatementDetailByIdAsync(statementId, cancellationToken);

        [Authorize]
        public static async Task<StatementChildCounts> GetChildCountsForStatement(
            Guid statementId,
            IDebService debService,
            IApplicationSettingsService applicationSettingsService,
            IDmsService dmsService,
            CancellationToken cancellationToken)
        {
            var counts = await debService.GetChildCountsForStatementAsync(statementId, cancellationToken);

            var debLibraryId = applicationSettingsService.GetLibraryId(DebHelper.Dms.Libraries.DebDocuments);
            var commonLibraryId = applicationSettingsService.GetLibraryId(DebHelper.Dms.Libraries.CommonDocuments);

            var debDocumentCount = await dmsService.GetEntityDocumentCountAsync(debLibraryId, statementId);
			//TODO: re-work after linking to common evidence is done.
			//var commonDocumentCount = await dmsService.GetEntityDocumentCountAsync(commonLibraryId, scopeId);
			var commonDocumentCount = 0;

			counts.EvidencesCount = debDocumentCount + commonDocumentCount;

            return counts;
        }
    }
}
