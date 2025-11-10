using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScopeExportView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        { 
            migrationBuilder.Sql(@"
                CREATE VIEW [deb].[vw_ScopeExport]
                AS
                SELECT
                  sc.[EntityId],
				  eh.[SerialNumber],
                  eh.[Title],
				  eh.[Description],
                  eh.[OwnedById],
				  vp.[Title] AS [OwnedBy],
                  eh.[CreatedDate],
                  eh.[LastModifiedDate],
				  vw.StateTitle AS [Status],
                  (SELECT COUNT(DISTINCT svr.[StandardVersionId])
				   FROM [deb].[StandardVersionRequirement] svr
				   INNER JOIN [deb].[Requirement] r ON svr.[RequirementId] = r.[EntityId]
				   INNER JOIN [deb].[ScopeRequirement] sr ON r.[EntityId] = sr.RequirementId AND sr.[ScopeId] = sc.[EntityID]
				  ) AS StandardVersionCount,
				  sc.TargetImplementationDate
                FROM [deb].[Scope] sc
                INNER JOIN [common].[EntityHead] eh on sc.[EntityId] = eh.[EntityId]
				LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
				LEFT JOIN [common].[XDB_CIS_View_Post] vp ON eh.[OwnedById] = vp.[ID]
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP VIEW IF EXISTS [deb].[vw_ScopeExport];
            ");
        }
    }
}
