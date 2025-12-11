using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixStandardVersionTitleOnStandardVersionSummaryView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StandardVersionSummary]
                AS
                SELECT 
					sv.EntityId,
					sv.StandardId,
                    st.[Title] AS StandardTitle,
                    eh.[Title] AS [Version],
                    st.[Title] + sv.Delimiter + eh.Title AS StandardVersionTitle,
                    sv.[EffectiveStartDate],
                    sv.[EffectiveEndDate],
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
                ALTER VIEW [deb].[vw_StandardVersionSummary]
                AS
                SELECT 
					sv.EntityId,
					sv.StandardId,
                    st.[Title] AS StandardTitle,
                    eh.[Title] AS [Version],
                    eh.[Description] AS StandardVersionTitle,
                    sv.[EffectiveStartDate],
                    sv.[EffectiveEndDate],
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
    }
}
