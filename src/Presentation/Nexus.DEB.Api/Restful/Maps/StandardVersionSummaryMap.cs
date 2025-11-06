using CsvHelper.Configuration;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.Restful.Maps
{
    public class StandardVersionSummaryMap : ClassMap<StandardVersionSummary>
    {
        public StandardVersionSummaryMap()
        {
            Map(m => m.EntityId);
            Map(m => m.StandardTitle).Name("Standard Title");
            Map(m => m.Version);
            Map(m => m.Title);
            Map(m => m.Status);
            Map(m => m.EffectiveFrom).Name("Effective From Date").TypeConverterOption.Format("yyyy-MM-dd");
            Map(m => m.EffectiveTo).Name("Effective To Date").TypeConverterOption.Format("yyyy-MM-dd");
            Map(m => m.NumberOfLinkedScopes).Name("Linked Scopes");
        }
    }
}
