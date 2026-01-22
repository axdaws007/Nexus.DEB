using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewScopeDetailView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
/****** Object:  View [deb].[vw_ScopeDetail]    Script Date: 22/01/2026 09:31:34 ******/
DROP VIEW IF EXISTS  [deb].[vw_ScopeDetail]
GO

/****** Object:  View [deb].[vw_ScopeDetail]    Script Date: 22/01/2026 09:31:34 ******/
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
/****** Object:  View [deb].[vw_ScopeDetail]    Script Date: 22/01/2026 09:31:34 ******/
DROP VIEW IF EXISTS  [deb].[vw_ScopeDetail]
GO
");
		}
    }
}
