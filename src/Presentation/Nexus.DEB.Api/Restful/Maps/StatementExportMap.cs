using CsvHelper.Configuration;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.Restful.Maps
{
    public class StatementExportMap : ClassMap<StatementExport>
    {
        public StatementExportMap()
        {
            Map(m => m.SerialNumber);
            Map(m => m.Title);
            Map(m => m.Description);
            Map(m => m.LastModifiedDate).Name("Last Modified").TypeConverterOption.Format("yyyy-MM-dd HH:mm:ss");
            Map(m => m.OwnedBy).Name("Owner");
            Map(m => m.RequirementSerialNumbers).Name("Requirement IDs");
            Map(m => m.Status);
        }
    }
}
