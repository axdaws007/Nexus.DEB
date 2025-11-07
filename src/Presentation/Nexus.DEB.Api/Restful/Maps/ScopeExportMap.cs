using CsvHelper.Configuration;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.Restful.Maps
{
    public class ScopeExportMap : ClassMap<ScopeExport>
    {
        public ScopeExportMap()
        {
            Map(m => m.SerialNumber);
            Map(m => m.Title);
            Map(m => m.Description);
            Map(m => m.OwnedBy).Name("Owner");
            Map(m => m.NumberOfLinkedStandardVersions).Name("Linked Standard Versions");
            Map(m => m.CreatedDate).Name("Created").TypeConverterOption.Format("yyyy-MM-dd HH:mm:ss");
            Map(m => m.LastModifiedDate).Name("Last Modified").TypeConverterOption.Format("yyyy-MM-dd HH:mm:ss");
            Map(m => m.Status);
            Map(m => m.TargetImplementationDate).Name("Target Implementation Date").TypeConverterOption.Format("yyyy-MM-dd");
        }
    }
}
