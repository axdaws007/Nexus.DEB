using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StandardVersionColumnChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reference",
                schema: "deb",
                table: "StandardVersion");

            migrationBuilder.DropColumn(
                name: "UseVersionPrefix",
                schema: "deb",
                table: "StandardVersion");

            migrationBuilder.AddColumn<string>(
                name: "Delimiter",
                schema: "deb",
                table: "StandardVersion",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StandardVersionExport]
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
                    sv.Delimiter,
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
                ALTER VIEW [deb].[vw_StandardVersionSummary]
                AS
                SELECT 
					sv.EntityId,
					sv.StandardId,
                    st.[Title] AS StandardTitle,
                    CONCAT(sv.[MajorVersion], ':', sv.[MinorVersion]) AS Version,
                    eh.[Title] AS StandardVersionTitle,
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

            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StandardVersionExport]
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

            migrationBuilder.DropColumn(
                name: "Delimiter",
                schema: "deb",
                table: "StandardVersion");

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                schema: "deb",
                table: "StandardVersion",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "UseVersionPrefix",
                schema: "deb",
                table: "StandardVersion",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
