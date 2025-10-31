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
                .Resolve(context =>
                {
                    var standardVersionSummary = context.Parent<StandardVersionSummary>();

                    return _pawsService.GetStatusForEntity(standardVersionSummary.StandardVersionId);
                });

            base.Configure(descriptor);
        }
    }
}
