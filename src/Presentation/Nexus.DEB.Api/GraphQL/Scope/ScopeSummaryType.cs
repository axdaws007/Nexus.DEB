using Nexus.DEB.Api.GraphQL.Cis;
using Nexus.DEB.Api.GraphQL.Paws;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL.Scope
{
    public class ScopeSummaryType : ObjectType<ScopeSummary>
    {
        private readonly IPawsService _pawsService;

        public ScopeSummaryType(IPawsService pawsService)
        {
            _pawsService = pawsService;
        }

        protected override void Configure(IObjectTypeDescriptor<ScopeSummary> descriptor)
        {
            descriptor
                .Field("ownedBy")
                .ResolveWith<CisResolver>(context => context.GetCisOwnedByNameAsync(default, default, default));

            descriptor
                .Field("status")
                .ResolveWith<PawsResolver>(context => context.GetCurrentPawsStatusAsync(default, default, default));

            base.Configure(descriptor);
        }
    }
}
