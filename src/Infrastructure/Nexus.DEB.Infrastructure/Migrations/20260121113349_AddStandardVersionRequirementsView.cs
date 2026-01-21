using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStandardVersionRequirementsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
/****** Object:  View [deb].[vw_StandardVersionRequirements]    Script Date: 20/01/2026 13:33:01 ******/
DROP VIEW IF EXISTS [deb].[vw_StandardVersionRequirements]
GO

/****** Object:  View [deb].[vw_StandardVersionRequirements]    Script Date: 20/01/2026 13:33:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [deb].[vw_StandardVersionRequirements]
AS

	WITH StandardVersions AS (
		SELECT 
			ehSV.EntityId,
			ehSV.Title
		FROM 
			deb.StandardVersion sv
			JOIN common.entityHead ehSV on ehSV.EntityID = sv.EntityID
		WHERE 
			ehSV.IsRemoved = 0 AND ehSV.IsArchived = 0
	),
	Sections AS (
		SELECT
			sect.Id, 
			TRIM(CASE WHEN sect.IsReferenceDisplayed = 1 THEN sect.Reference + '. ' ELSE '' END + CASE WHEN sect.IsTitleDisplayed = 1 THEN sect.Title ELSE '' END) [Title],
			sect.StandardVersionId
		FROM
			deb.Section sect
	)
	SELECT
		r.EntityId [RequirementId], 
		ehR.SerialNumber, 
		ehR.Title,
		sv.EntityID [StandardVersionId],
		ehsv.Title [StandardVersion],
		sect.Id [SectionId], 
		sect.Title [Section]
	FROM
		deb.Requirement r
		JOIN common.EntityHead ehR on ehR.EntityID = r.EntityID
		LEFT JOIN deb.StandardVersionRequirement svr  on svr.RequirementId = r.EntityID
		LEFT JOIN StandardVersions sv on svr.StandardVersionId = sv.EntityID
		LEFT JOIN common.entityHead ehSV on ehSV.EntityID = sv.EntityID 
		LEFT JOIN deb.SectionRequirement sectR ON sectR.RequirementID = r.EntityID AND sectR.IsEnabled = 1
		LEFT JOIN Sections sect on sect.Id = sectR.SectionID AND sect.StandardVersionId = sv.EntityID
	WHERE 
		ehR.IsRemoved = 0 AND ehR.IsArchived = 0
GO
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"
DROP VIEW IF EXISTS [deb].[vw_StandardVersionRequirements]
GO
");
		}
    }
}
