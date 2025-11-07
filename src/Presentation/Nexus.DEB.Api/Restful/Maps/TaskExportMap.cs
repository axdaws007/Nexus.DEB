using CsvHelper.Configuration;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.Restful.Maps
{
    public class TaskExportMap : ClassMap<TaskExport>
    {
        public TaskExportMap()
        {
            Map(m => m.SerialNumber).Name("Task ID");
            Map(m => m.Title);
            Map(m => m.Description);
            Map(m => m.OwnedBy).Name("Owner");
            Map(m => m.DueDate).Name("Due Date").TypeConverterOption.Format("yyyy-MM-dd");
            Map(m => m.TaskTypeTitle).Name("Type");
            Map(m => m.Status);
            Map(m => m.StatementSerialNumber).Name("Parent Statement ID");
        }
    }
}
