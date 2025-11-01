using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScopeSummaryView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW [deb].[vw_ScopeSummary]
                AS
                SELECT
                  sc.[Id],
                  eh.[Title],
                  eh.[OwnedById],
                  eh.[CreatedDate],
                  eh.[LastModifiedDate],
                  COUNT(DISTINCT svr.[StandardVersionId]) AS StandardVersionCount
                FROM [deb].[Scope] sc
                INNER JOIN [common].[EntityHead] eh on sc.[Id] = eh.[Id]
                LEFT JOIN [deb].[ScopeRequirement] sr ON sc.[Id] = sr.[ScopeId]
                LEFT JOIN [deb].[Requirement] r ON sr.[RequirementId] = r.[Id]
                LEFT JOIN [deb].[StandardVersionRequirement] svr ON r.[Id] = svr.[StandardVersionId]
                GROUP BY 
                  sc.[Id],
                  eh.[Title],
                  eh.[OwnedById],
                  eh.[CreatedDate],
                  eh.[LastModifiedDate]
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP VIEW IF EXISTS [deb].[vw_ScopeSummary];
            ");
        }
    }
}
