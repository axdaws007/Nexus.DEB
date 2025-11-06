using CsvHelper.Configuration;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.Restful.Maps
{
    public class TaskSummaryMap : ClassMap<TaskSummary>
    {
        public TaskSummaryMap()
        {
            Map(m => m.EntityId);
            Map(m => m.SerialNumber).Name("Task ID");
            Map(m => m.Title);
            Map(m => m.OwnedBy).Name("Owner");
            Map(m => m.DueDate).Name("Due Date").TypeConverterOption.Format("yyyy-MM-dd");
            Map(m => m.TaskTypeTitle).Name("Type");
            Map(m => m.Status);

            Map(m => m.StatusId).Ignore();
            Map(m => m.OwnedById).Ignore();
        }
    }
}
