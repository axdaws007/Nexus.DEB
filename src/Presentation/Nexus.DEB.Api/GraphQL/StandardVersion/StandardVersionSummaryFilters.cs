using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class StandardVersionSummaryFilters
    {
        [ID]
        public ICollection<short>? StandardIds { get; set; }

        [ID]
        public ICollection<int>? StatusIds { get; set; }

        public DateOnly? EffectiveFromDate { get; set; }

        public DateOnly? EffectiveToDate { get; set; }
    }
}
