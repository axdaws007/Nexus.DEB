using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStandardVersionExportView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW [deb].[vw_StandardVersionExport]
                AS
                SELECT 
					sv.EntityId,
					sv.StandardId,
                    st.[Title] AS StandardTitle,
					eh.[SerialNumber],
                    eh.[Title] AS StandardVersionTitle,
					eh.[Description],
                    sv.[EffectiveStartDate],
                    sv.[EffectiveEndDate],
                    sv.Reference,
                    sv.MajorVersion,
					sv.MinorVersion,
                    eh.[LastModifiedDate],
					vw.StateID AS StatusId,
					vw.StateTitle AS [Status],
					(SELECT COUNT(DISTINCT sc.EntityId)
					 FROM [deb].[Scope] sc
					 INNER JOIN [deb].[ScopeRequirement] scr ON sc.[EntityId]= scr.ScopeId
					 INNER JOIN [deb].[StandardVersionRequirement] svr ON scr.[RequirementId] = svr.RequirementId AND svr.[StandardVersionId] = sv.[EntityId]) AS ScopeCount
                FROM [deb].[StandardVersion] sv
                INNER JOIN [deb].[Standard] st ON sv.[StandardId] = st.[Id]
                INNER JOIN [common].[EntityHead] eh ON sv.[EntityId] = eh.[EntityId]
				LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP VIEW IF EXISTS [deb].[vw_StandardVersionExport];
            ");
        }
    }
}
