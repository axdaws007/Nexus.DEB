using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnToScopeSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
/****** Object:  View [deb].[vw_ScopeSummary]    Script Date: 06/02/2026 10:58:00 ******/
DROP VIEW IF EXISTS [deb].[vw_ScopeSummary]
GO

/****** Object:  View [deb].[vw_ScopeSummary]    Script Date: 06/02/2026 10:58:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [deb].[vw_ScopeSummary]
AS

SELECT
    sc.[EntityId],
	eh.[SerialNumber],
    eh.[Title],
	eh.[Description],
    eh.[OwnedById],
	vp.[Title] AS [OwnedBy],
    eh.[CreatedDate],
    eh.[LastModifiedDate],
	vw.StateTitle AS [Status],
	eh.EntityTypeTitle,
    (SELECT COUNT(DISTINCT svr.[StandardVersionId])
	FROM [deb].[StandardVersionRequirement] svr
	INNER JOIN [deb].[Requirement] r ON svr.[RequirementId] = r.[EntityId]
	INNER JOIN [deb].[ScopeRequirement] sr ON r.[EntityId] = sr.RequirementId AND sr.[ScopeId] = sc.[EntityID]
	) AS StandardVersionCount
FROM [deb].[Scope] sc
INNER JOIN [common].[EntityHead] eh on sc.[EntityId] = eh.[EntityId]
LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
LEFT JOIN [common].[XDB_CIS_View_Post] vp ON eh.[OwnedById] = vp.[ID]
            
GO
");

            migrationBuilder.Sql(@"
/****** Object:  View [deb].[vw_ScopeDetail]    Script Date: 06/02/2026 11:13:04 ******/
DROP VIEW IF EXISTS [deb].[vw_ScopeDetail]
GO

/****** Object:  View [deb].[vw_ScopeDetail]    Script Date: 06/02/2026 11:13:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [deb].[vw_ScopeDetail]
AS
    SELECT 
	    ehD.EntityId,
		ehD.EntityTypeTitle,
		ehD.SerialNumber,
		ehD.Title,
		eh.Description,
		ehD.CreatedDate,
		ehD.CreatedByPostTitle,
		eh.OwnedById,
		ehD.OwnedByPostTitle,
		ehD.LastModifiedByPostTitle,
		ehD.LastModifiedDate,
		sv.TargetImplementationDate,
		eh.IsRemoved,
		eh.IsArchived
    FROM 
		[common].[vw_entityHeadDetail] ehD
		INNER JOIN common.EntityHead eh ON eh.EntityID = ehD.EntityId
		INNER JOIN [deb].[Scope] sv ON ehD.[EntityID] = sv.[EntityID]
            
GO
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"
/****** Object:  View [deb].[vw_ScopeSummary]    Script Date: 06/02/2026 10:58:00 ******/
DROP VIEW IF EXISTS [deb].[vw_ScopeSummary]
GO

/****** Object:  View [deb].[vw_ScopeSummary]    Script Date: 06/02/2026 10:58:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [deb].[vw_ScopeSummary]
AS

SELECT
    sc.[EntityId],
	eh.[SerialNumber],
    eh.[Title],
	eh.[Description],
    eh.[OwnedById],
	vp.[Title] AS [OwnedBy],
    eh.[CreatedDate],
    eh.[LastModifiedDate],
	vw.StateTitle AS [Status],
    (SELECT COUNT(DISTINCT svr.[StandardVersionId])
	FROM [deb].[StandardVersionRequirement] svr
	INNER JOIN [deb].[Requirement] r ON svr.[RequirementId] = r.[EntityId]
	INNER JOIN [deb].[ScopeRequirement] sr ON r.[EntityId] = sr.RequirementId AND sr.[ScopeId] = sc.[EntityID]
	) AS StandardVersionCount
FROM [deb].[Scope] sc
INNER JOIN [common].[EntityHead] eh on sc.[EntityId] = eh.[EntityId]
LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
LEFT JOIN [common].[XDB_CIS_View_Post] vp ON eh.[OwnedById] = vp.[ID]
            
GO
");

			migrationBuilder.Sql(@"
/****** Object:  View [deb].[vw_ScopeDetail]    Script Date: 06/02/2026 11:13:04 ******/
DROP VIEW IF EXISTS [deb].[vw_ScopeDetail]
GO

/****** Object:  View [deb].[vw_ScopeDetail]    Script Date: 06/02/2026 11:13:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [deb].[vw_ScopeDetail]
AS
    SELECT 
	    ehD.EntityId,
		ehD.EntityTypeTitle,
		ehD.SerialNumber,
		ehD.Title,
		eh.Description,
		ehD.CreatedDate,
		ehD.CreatedByPostTitle,
		ehD.OwnedByPostTitle,
		ehD.LastModifiedByPostTitle,
		ehD.LastModifiedDate,
		sv.TargetImplementationDate,
		eh.IsRemoved,
		eh.IsArchived
    FROM 
		[common].[vw_entityHeadDetail] ehD
		INNER JOIN common.EntityHead eh ON eh.EntityID = ehD.EntityId
		INNER JOIN [deb].[Scope] sv ON ehD.[EntityID] = sv.[EntityID]
            
GO
");
		}
    }
}
