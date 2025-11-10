using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlterStatementSummaryViewAddOwnedByColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StatementSummary]
                AS
                SELECT
                    st.[EntityId],
                    eh.[SerialNumber],
                    eh.[Title],
                    eh.[LastModifiedDate],
					eh.[OwnedById],
    				vp.[Title] AS [OwnedBy],
                    (
                        SELECT STRING_AGG(ehr.SerialNumber, ', ')
                        FROM [common].[EntityHead] ehr
                        INNER JOIN [deb].[Requirement] r ON ehr.[EntityId] = r.[EntityId]
                        INNER JOIN [deb].[StatementRequirement] str ON r.[EntityId] = str.[RequirementId]
                        WHERE str.[StatementId] = st.[EntityId]
                    ) AS RequirementSerialNumbers,
					vw.[StateID] AS [StatusId],
					vw.[StateTitle] AS [Status]
                FROM [deb].[Statement] st
                INNER JOIN [common].[EntityHead] eh ON st.[EntityId] = eh.[EntityId]
				LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
				LEFT JOIN [common].[XDB_CIS_View_Post] vp ON eh.[OwnedById] = vp.[ID]
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StatementSummary]
                AS
                SELECT
                    st.[EntityId],
                    eh.[SerialNumber],
                    eh.[Title],
                    eh.[LastModifiedDate],
					eh.[OwnedById],
                    (
                        SELECT STRING_AGG(ehr.SerialNumber, ', ')
                        FROM [common].[EntityHead] ehr
                        INNER JOIN [deb].[Requirement] r ON ehr.[EntityId] = r.[EntityId]
                        INNER JOIN [deb].[StatementRequirement] str ON r.[EntityId] = str.[RequirementId]
                        WHERE str.[StatementId] = st.[EntityId]
                    ) AS RequirementSerialNumbers,
					vw.[StateID] AS [StatusId],
					vw.[StateTitle] AS [Status]
                FROM [deb].[Statement] st
                INNER JOIN [common].[EntityHead] eh ON st.[EntityId] = eh.[EntityId]
				LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
            ");
        }

    }
}
