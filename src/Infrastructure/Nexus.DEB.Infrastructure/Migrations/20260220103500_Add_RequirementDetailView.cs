using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_RequirementDetailView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
/****** Object:  View [deb].[vw_RequirementDetail]    Script Date: 20/02/2026 09:31:34 ******/
DROP VIEW IF EXISTS  [deb].[vw_RequirementDetail]
GO

/****** Object:  View [deb].[vw_RequirementDetail]    Script Date: 20/02/2026 09:31:34 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [deb].[vw_RequirementDetail]
AS
    SELECT 
	                eh.[EntityId],
	                eh.[Title],
	                eh.[Description],
	                eh.[SerialNumber],
	                eh.[CreatedDate],
	                eh.[LastModifiedDate],
	                eh.[IsRemoved],
	                eh.[IsArchived],
	                eh.[EntityTypeTitle],
					r.ComplianceWeighting,
					r.EffectiveStartDate,
					r.EffectiveEndDate,
					r.IsReferenceDisplayed,
					r.IsTitleDisplayed,
					r.RequirementCategoryId,
					r.RequirementTypeId,
	                vp_cr.[Title] AS [CreatedBy],
	                vp_lm.[Title] AS [LastModifiedBy],
	                vp_ow.[Title] AS [OwnedBy],
					eh.OwnedById
                FROM [common].[EntityHead] eh
                INNER JOIN [deb].[Requirement] r ON eh.[EntityID] = r.[EntityID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_cr ON eh.[CreatedById] = vp_cr.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_lm ON eh.[LastModifiedById] = vp_lm.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_ow ON eh.[OwnedById] = vp_ow.[ID]
            
GO
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
/****** Object:  View [deb].[vw_RequirementDetail]    Script Date: 20/02/2026 09:31:34 ******/
DROP VIEW IF EXISTS  [deb].[vw_RequirementDetail]
GO
");
        }
    }
}
