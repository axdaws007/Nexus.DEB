using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class StandardVersionSummaryFilters
    {
        [ID]
        public ICollection<short>? StandardIds { get; set; }

        [ID]
        public ICollection<int>? StatusIds { get; set; }

        public DateTime? EffectiveFromDate { get; set; }

        public DateTime? EffectiveToDate { get; set; }
    }
}
