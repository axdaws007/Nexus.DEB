using CsvHelper.Configuration;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.Restful.Maps
{
    public class RequirementExportMap : ClassMap<RequirementExport>
    {
        public RequirementExportMap()
        {
            Map(m => m.SerialNumber).Name("Requirement ID");
            Map(m => m.Title);
            Map(m => m.Description);
            Map(m => m.LastModifiedDate).Name("Last Modified").TypeConverterOption.Format("yyyy-MM-dd HH:mm:ss");
            Map(m => m.SectionReferences).Name("Section ID");
            Map(m => m.Status);
            Map(m => m.EffectiveStartDate).Name("Effective From").TypeConverterOption.Format("yyyy-MM-dd");
            Map(m => m.EffectiveEndDate).Name("Effective To").TypeConverterOption.Format("yyyy-MM-dd");
            Map(m => m.RequirementCategoryTitle).Name("Category");
            Map(m => m.RequirementTypeTitle).Name("Type");
            Map(m => m.ComplianceWeighting).Name("Compliance Weighting");
        }
    }
}
