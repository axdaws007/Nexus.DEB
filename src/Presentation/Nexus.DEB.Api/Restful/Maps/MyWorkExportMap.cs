using CsvHelper.Configuration;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.Restful.Maps
{
    public class MyWorkExportMap : ClassMap<MyWorkDetailItem>
    {
        public MyWorkExportMap()
        {
            Map(m => m.SerialNumber).Name("Serialnumber");
            Map(m => m.Title);
            Map(m => m.CreatedDate).Name("Created Date");
            Map(m => m.ModifiedDate).Name("Modified Date");
            Map(m => m.OwnerGroup).Name("Group");
            Map(m => m.OwnerPost).Name("Owner");
            Map(m => m.PendingActivityOwners).Name("Activity Owners");
            Map(m => m.PendingActivityList).Name("Pending Activities");
            Map(m => m.TransferDates).Name("Assigned Dates");
            Map(m => m.DueDate).Name("Due Date");
            Map(m => m.ReviewDate).Name("Review Date");

            Map(m => m.EntityID).Ignore();
            Map(m => m.EntityTypeTitle).Ignore();
            Map(m => m.ModuleID).Ignore();
        }
    }
}
