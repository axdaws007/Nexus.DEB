using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StandardVersionUpdatesForCreateEdit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Add EntityTypeTitle to deb.vw_StandardVersionSummary
			migrationBuilder.Sql(@"
/****** Object:  View [deb].[vw_StandardVersionSummary]    Script Date: 11/02/2026 10:25:17 ******/
DROP VIEW IF EXISTS [deb].[vw_StandardVersionSummary]
GO

/****** Object:  View [deb].[vw_StandardVersionSummary]    Script Date: 11/02/2026 10:25:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [deb].[vw_StandardVersionSummary]
AS
SELECT 
	sv.EntityId,
	eh.EntityTypeTitle,
	sv.StandardId,
    st.[Title] AS StandardTitle,
    sv.[VersionTitle] AS [Version],
    eh.Title AS StandardVersionTitle,
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
WHERE eh.IsRemoved = 0 AND eh.IsArchived = 0
            
GO
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			// Remove EntityTypeTitle from deb.vw_StandardVersionSummary
			migrationBuilder.Sql(@"
/****** Object:  View [deb].[vw_StandardVersionSummary]    Script Date: 11/02/2026 10:25:17 ******/
DROP VIEW IF EXISTS [deb].[vw_StandardVersionSummary]
GO

/****** Object:  View [deb].[vw_StandardVersionSummary]    Script Date: 11/02/2026 10:25:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [deb].[vw_StandardVersionSummary]
AS
SELECT 
	sv.EntityId,
	sv.StandardId,
    st.[Title] AS StandardTitle,
    sv.[VersionTitle] AS [Version],
    eh.Title AS StandardVersionTitle,
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
            
GO
");
		}
    }
}
