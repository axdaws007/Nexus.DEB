using Nexus.DEB.Api.GraphQL.Paws;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL.StandardVersion
{
    public class StandardVersionSummaryType : ObjectType<StandardVersionSummary>
    {
        private readonly IPawsService _pawsService;

        public StandardVersionSummaryType(IPawsService pawsService)
        {
            _pawsService = pawsService;
        }

        protected override void Configure(IObjectTypeDescriptor<StandardVersionSummary> descriptor)
        {
            descriptor
                .Field("status")
                .ResolveWith<PawsResolver>(context => context.GetCurrentPawsStatusAsync(default, default, default));

            base.Configure(descriptor);
        }
    }
}
