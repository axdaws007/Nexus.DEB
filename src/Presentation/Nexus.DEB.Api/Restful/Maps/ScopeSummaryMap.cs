using CsvHelper.Configuration;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.Restful.Maps
{
    public class ScopeSummaryMap : ClassMap<ScopeSummary>
    {
        public ScopeSummaryMap()
        {
            Map(m => m.EntityId);
            Map(m => m.Title);
            Map(m => m.OwnedBy).Name("Owner");
            Map(m => m.NumberOfLinkedStandardVersions).Name("Linked Standard Versions");
            Map(m => m.CreatedDate).Name("Created").TypeConverterOption.Format("yyyy-MM-dd HH:mm:ss");
            Map(m => m.LastModifiedDate).Name("Last Modified").TypeConverterOption.Format("yyyy-MM-dd HH:mm:ss");
            Map(m => m.Status);

            Map(m => m.OwnedById).Ignore();
        }
    }
}
