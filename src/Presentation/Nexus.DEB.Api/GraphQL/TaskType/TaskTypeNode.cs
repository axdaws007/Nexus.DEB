using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [Node]
    [ExtendObjectType<TaskType>]
    public class TaskTypeNode
    {
        [DataLoader]
        public static async Task<IReadOnlyDictionary<short, TaskType>> GetTaskTypeByIdAsync(
            IReadOnlyList<short> keys,
            IDebService debService,
            CancellationToken cancellationToken)
            => await debService.GetTaskTypes()
                .Where(x => keys.Contains(x.Id))
                .ToDictionaryAsync(t => t.Id, cancellationToken);
    }
}
