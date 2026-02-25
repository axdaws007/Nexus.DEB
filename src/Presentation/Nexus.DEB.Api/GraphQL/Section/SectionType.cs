using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{

    // We can do this this way because the LINQ query getting the data uses Includes so the requirements are not lazy loaded and the data will be in memory.
    [ObjectType<Section>]
    public static partial class SectionType
    {
        public static int GetRequirementCount([Parent] Section section)
            => section.SectionRequirements?.Count ?? 0;
    }
}
