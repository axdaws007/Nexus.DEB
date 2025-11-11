using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRequirementSummaryView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP VIEW IF EXISTS [deb].[vw_RequirementSummary];
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER VIEW [deb].[vw_RequirementSummary]
                AS
                SELECT
                    r.[EntityId],
                    eh.[SerialNumber],
                    eh.[Title],
                    eh.[LastModifiedDate],
                    (
                        SELECT ISNULL(STRING_AGG(s.[Reference], ', '), '')
                        FROM [deb].[SectionRequirement] sr
                        INNER JOIN [deb].[Section] s ON sr.[SectionID] = s.[Id]
                        WHERE sr.[RequirementID] = r.[EntityId]
                    ) AS SectionReferences,
				vw.[StateID] AS [StatusId],
				vw.[StateTitle] AS [Status] 
                FROM [deb].[Requirement] r
                INNER JOIN [common].[EntityHead] eh ON r.[EntityId] = eh.[EntityId]
				LEFT JOIN [common].vwPawsState vw ON r.[EntityID] = vw.[EntityID]
            ");
        }
    }
}
