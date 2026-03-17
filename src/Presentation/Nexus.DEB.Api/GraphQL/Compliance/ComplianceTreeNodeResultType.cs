using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class ComplianceTreeNodeResultType : ObjectType<ComplianceTreeNodeResult>
    {
        protected override void Configure(IObjectTypeDescriptor<ComplianceTreeNodeResult> descriptor)
        {
            descriptor
                .Field("requirementCompletionPercentage")
                .Type<DecimalType>()
                .Resolve(context =>
                {
                    var result = context.Parent<ComplianceTreeNodeResult>();
                    var node = result.Node;

                    // Only applicable to nodes with aggregate data
                    if (node.TotalRequirementCount is null or 0)
                        return null;

                    var terminalCount = node.Summaries
                        .Where(s => s.ChildNodeType == ComplianceNodeTypes.Requirement
                                 && s.ComplianceState?.IsTerminal == true)
                        .Sum(s => s.Count);

                    return Math.Round(
                        (decimal)terminalCount / node.TotalRequirementCount.Value * 100,
                        1);
                });

            descriptor
                .Field("sectionCompletionPercentage")
                .Type<DecimalType>()
                .Resolve(context =>
                {
                    var result = context.Parent<ComplianceTreeNodeResult>();
                    var node = result.Node;

                    // Only applicable to nodes with aggregate data
                    if (node.TotalSectionCount is null or 0)
                        return null;

                    var terminalCount = node.Summaries
                        .Where(s => s.ChildNodeType == ComplianceNodeTypes.Section
                                 && s.ComplianceState?.IsTerminal == true)
                        .Sum(s => s.Count);

                    return Math.Round(
                        (decimal)terminalCount / node.TotalSectionCount.Value * 100,
                        1);
                });
        }
    }
}