using CsvHelper.Configuration;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.Restful.Maps
{
    public class StandardVersionExportMap : ClassMap<StandardVersionExport>
    {
        public StandardVersionExportMap()
        {
            Map(m => m.StandardTitle).Name("Standard");
            Map(m => m.VersionTitle).Name("Version");
            Map(m => m.Title);
            Map(m => m.Status);
            Map(m => m.EffectiveStartDate).Name("Active From").TypeConverterOption.Format("yyyy-MM-dd");
            Map(m => m.EffectiveEndDate).Name("Active To").TypeConverterOption.Format("yyyy-MM-dd");
            Map(m => m.LastModifiedDate).Name("Last Modified").TypeConverterOption.Format("yyyy-MM-dd HH:mm:ss");
            Map(m => m.NumberOfLinkedScopes).Name("Linked Scopes");
        }
    }
}
