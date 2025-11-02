using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequirementSummaryView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW [deb].[vw_RequirementSummary]
                AS
                SELECT
                    r.[Id],
                    eh.[SerialNumber],
                    eh.[Title],
                    eh.[LastModifiedDate],
                    (
                        SELECT STRING_AGG(s.[Reference], ', ')
                        FROM [deb].[SectionRequirement] sr
                        INNER JOIN [deb].[Section] s ON sr.[SectionID] = s.[Id]
                        WHERE sr.[RequirementID] = r.[Id]
                    ) AS SectionReferences
                FROM [deb].[Requirement] r
                INNER JOIN [common].[EntityHead] eh on r.[Id] = eh.[Id]
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP VIEW IF EXISTS [deb].[vw_RequirementSummary];
            ");
        }

    }
}
