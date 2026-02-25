using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequirementSectionSummaryView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER VIEW [deb].[vw_RequirementSectionSummary]
                AS
                SELECT
                    r.[EntityId],
                    eh.[SerialNumber],
                    eh.[Title],
                    eh.[Description],
					vw.[StateID] AS [StatusId],
					vw.[StateTitle] AS [Status],
					(SELECT COUNT(*) FROM [deb].[SectionRequirement] WHERE RequirementID = r.[EntityID]) AS NumberOfLinkedSections
                FROM [deb].[Requirement] r
                INNER JOIN [common].[EntityHead] eh ON r.[EntityId] = eh.[EntityId]
				LEFT JOIN [common].vwPawsState vw ON r.[EntityID] = vw.[EntityID]
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW [deb].[vw_RequirementSectionSummary]");
        }
    }
}
