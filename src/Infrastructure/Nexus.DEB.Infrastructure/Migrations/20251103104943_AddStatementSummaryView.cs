using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatementSummaryView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW [deb].[vw_StatementSummary]
                AS
                SELECT
                    st.[Id],
                    eh.[SerialNumber],
                    eh.[Title],
                    eh.[LastModifiedDate],
					eh.[OwnedById],
                    (
                        SELECT STRING_AGG(ehr.SerialNumber, ', ')
                        FROM [common].[EntityHead] ehr
                        INNER JOIN [deb].[Requirement] r ON ehr.[Id] = r.[Id]
                        INNER JOIN [deb].[StatementRequirement] str ON r.[Id] = str.[RequirementId]
                        WHERE str.[StatementId] = st.[Id]
                    ) AS RequirementSerialNumbers
                FROM [deb].[Statement] st
                INNER JOIN [common].[EntityHead] eh on st.[Id] = eh.[Id]
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP VIEW IF EXISTS [deb].[vw_StatementSummary];
            ");
        }
    }
}
