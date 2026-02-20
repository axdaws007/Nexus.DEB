using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class RequirementCategoryQueries
    {
        [Authorize]
        public static IQueryable<RequirementCategory> GetRequirementCategories(
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetRequirementCategories(); 
        
        [Authorize]
        public async static Task<ICollection<FilterItem>> GetRequirementCategoriesLookupAsync(
            IDebService debService,
            IResolverContext resolverContext,
            CancellationToken cancellationToken)
            => await debService.GetRequirementCategoriesLookupAsync(cancellationToken);


    }
}
