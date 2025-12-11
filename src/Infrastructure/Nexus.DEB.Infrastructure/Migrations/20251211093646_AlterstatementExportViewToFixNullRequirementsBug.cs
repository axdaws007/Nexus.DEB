using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlterstatementExportViewToFixNullRequirementsBug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StatementExport]
                AS
                SELECT
                    st.[EntityId],
                    eh.[SerialNumber],
                    eh.[Title],
					eh.[Description],
                    eh.[LastModifiedDate],
					eh.[OwnedById],
    				vp.[Title] AS [OwnedBy],
                    (
                        SELECT ISNULL(STRING_AGG(ehr.SerialNumber, ', '), '')
                        FROM [common].[EntityHead] ehr
                        INNER JOIN [deb].[Requirement] r ON ehr.[EntityId] = r.[EntityId]
                        INNER JOIN [deb].[StatementRequirementScope] srs ON r.[EntityId] = srs.[RequirementId]
                        WHERE srs.[StatementId] = st.[EntityId]
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
                ALTER VIEW [deb].[vw_StatementExport]
                AS
                SELECT
                    st.[EntityId],
                    eh.[SerialNumber],
                    eh.[Title],
					eh.[Description],
                    eh.[LastModifiedDate],
					eh.[OwnedById],
    				vp.[Title] AS [OwnedBy],
                    (
                        SELECT STRING_AGG(ehr.SerialNumber, ', ')
                        FROM [common].[EntityHead] ehr
                        INNER JOIN [deb].[Requirement] r ON ehr.[EntityId] = r.[EntityId]
                        INNER JOIN [deb].[StatementRequirementScope] srs ON r.[EntityId] = srs.[RequirementId]
                        WHERE srs.[StatementId] = st.[EntityId]
                    ) AS RequirementSerialNumbers,
					vw.[StateID] AS [StatusId],
					vw.[StateTitle] AS [Status]
                FROM [deb].[Statement] st
                INNER JOIN [common].[EntityHead] eh ON st.[EntityId] = eh.[EntityId]
				LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
				LEFT JOIN [common].[XDB_CIS_View_Post] vp ON eh.[OwnedById] = vp.[ID]
            ");
        }
    }
}
