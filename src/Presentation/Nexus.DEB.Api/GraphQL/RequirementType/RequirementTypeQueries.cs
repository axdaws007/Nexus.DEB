using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class RequirementTypeQueries
    {
        [Authorize]
        public static IQueryable<RequirementType> GetRequirementTypes(
            IDebService debService,
            IResolverContext resolverContext)
            => debService.GetRequirementTypes(); 
        
        [Authorize]
        public async static Task<ICollection<FilterItem>> GetRequirementTypesLookupAsync(
            IDebService debService,
            IResolverContext resolverContext,
            CancellationToken cancellationToken)
            => await debService.GetRequirementTypesLookupAsync(cancellationToken);


    }
}
