using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [Node]
    [ExtendObjectType<Standard>]
    public class StandardNode
    {
        [DataLoader]
        public static async Task<IReadOnlyDictionary<short, Standard>> GetStandardByIdAsync(
            IReadOnlyList<short> keys,
            IDebService debService,
            CancellationToken cancellationToken)
            => await debService.GetStandards()
                .Where(x => keys.Contains(x.Id))
                .ToDictionaryAsync(t => t.Id, cancellationToken);
    }
}
