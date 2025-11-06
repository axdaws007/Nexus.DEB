using CsvHelper.Configuration;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.Restful.Maps
{
    public class TaskSummaryMap : ClassMap<TaskSummary>
    {
        public TaskSummaryMap()
        {
            Map(m => m.EntityId).Name("EntityId");
            Map(m => m.TaskTypeTitle).Name("Task Type");
            Map(m => m.Title).Name("Title");
            Map(m => m.Status).Name("Status");
            Map(m => m.DueDate).Name("Due Date");

            Map(m => m.StatusId).Ignore();
        }
    }
}
