using CsvHelper.Configuration;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.Restful.Maps
{
    public class RequirementSummaryMap : ClassMap<RequirementSummary>
    {
        public RequirementSummaryMap()
        {
            Map(m => m.EntityId);
            Map(m => m.SerialNumber).Name("Requirement ID");
            Map(m => m.SectionReferences).Name("Section ID");
            Map(m => m.Title);
            Map(m => m.Status);
            Map(m => m.LastModifiedDate).Name("Last Modified").TypeConverterOption.Format("yyyy-MM-dd HH:mm:ss");

            Map(m => m.StatusId).Ignore();
        }
    }
}
