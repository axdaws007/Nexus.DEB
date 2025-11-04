using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Services
{
    public partial class DebService
    {
        public IQueryable<Standard> GetStandards()
        {
            var query = from s in _dbContext.Standards
                        where s.IsEnabled == true
                        orderby s.Ordinal
                        select s;

            return query;
        }

        public IQueryable<TaskType> GetTaskTypes()
        {
            var query = from s in _dbContext.TaskTypes
                        where s.IsEnabled == true
                        orderby s.Ordinal
                        select s;

            return query;
        }
    }
}
