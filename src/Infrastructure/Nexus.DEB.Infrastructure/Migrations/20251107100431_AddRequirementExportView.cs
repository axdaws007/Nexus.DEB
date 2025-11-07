using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequirementExportView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW [deb].[vw_RequirementExport]
                AS
                SELECT
                    r.[EntityId],
                    eh.[SerialNumber],
                    eh.[Title],
                    eh.[Description],
                    eh.[LastModifiedDate],
                    (
                        SELECT STRING_AGG(s.[Reference], ', ')
                        FROM [deb].[SectionRequirement] sr
                        INNER JOIN [deb].[Section] s ON sr.[SectionID] = s.[Id]
                        WHERE sr.[RequirementID] = r.[EntityId]
                    ) AS SectionReferences,
					vw.[StateID] AS [StatusId],
					vw.[StateTitle] AS [Status],
					r.[EffectiveStartDate],
					r.[EffectiveEndDate],
					rc.[Title] AS [RequirementCategoryTitle],
					rt.[Title] AS [RequirementTypeTitle],
					r.[ComplianceWeighting]
                FROM [deb].[Requirement] r
                INNER JOIN [common].[EntityHead] eh ON r.[EntityId] = eh.[EntityId]
				INNER JOIN [deb].[RequirementCategory] rc ON r.[RequirementCategoryId] = rc.[Id]
				INNER JOIN [deb].[RequirementType] rt ON r.[RequirementTypeId] = rt.[Id]
				LEFT JOIN [common].vwPawsState vw ON r.[EntityID] = vw.[EntityID]
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP VIEW IF EXISTS [deb].[vw_RequirementExport];
            ");
        }
    }
}
