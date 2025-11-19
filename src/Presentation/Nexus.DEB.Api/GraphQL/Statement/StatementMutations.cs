using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [MutationType]
    public static class StatementMutations
    {
        [Authorize]
        public static async Task<Statement?> CreateStatementAsync(
            IDebService debService,
            string statementText,
            DateTime? reviewDate,
            CancellationToken cancellationToken = default)
        {
            return new Statement();
        }
    }
}
