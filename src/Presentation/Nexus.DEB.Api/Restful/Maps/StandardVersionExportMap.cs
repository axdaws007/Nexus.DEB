using CsvHelper.Configuration;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.Restful.Maps
{
    public class StandardVersionExportMap : ClassMap<StandardVersionExport>
    {
        public StandardVersionExportMap()
        {
            Map(m => m.StandardTitle).Name("Standard Title");
            Map(m => m.Title);
            Map(m => m.Description);
            Map(m => m.EffectiveStartDate).Name("Effective From").TypeConverterOption.Format("yyyy-MM-dd");
            Map(m => m.EffectiveEndDate).Name("Effective To").TypeConverterOption.Format("yyyy-MM-dd");
            Map(m => m.Delimiter);
            Map(m => m.MajorVersion).Name("Major Version");
            Map(m => m.MinorVersion).Name("Minor Version");
            Map(m => m.LastModifiedDate).Name("Last Modified").TypeConverterOption.Format("yyyy-MM-dd HH:mm:ss");
            Map(m => m.Status);
            Map(m => m.NumberOfLinkedScopes).Name("Linked Scopes");
        }
    }
}
