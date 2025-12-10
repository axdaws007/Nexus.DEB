using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToStandardVersionDetailView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"/****** Object:  View [deb].[vw_StandardVersionDetail]    Script Date: 09/12/2025 10:15:03 ******/
DROP VIEW IF EXISTS [deb].[vw_StandardVersionDetail]
GO

/****** Object:  View [deb].[vw_StandardVersionDetail]    Script Date: 09/12/2025 10:15:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [deb].[vw_StandardVersionDetail]
AS
    SELECT 
	    ehD.EntityId,
		ehD.EntityTypeTitle,
		ehD.SerialNumber,
		s.Id [StandardId],
		s.Title [StandardTitle],
		sv.Delimiter,
		ehD.Title,
		eh.Description,
		ehD.CreatedDate,
		ehD.CreatedByPostTitle,
		ehD.OwnedByPostTitle,
		ehD.LastModifiedByPostTitle,
		ehD.LastModifiedDate,
		sv.MajorVersion,
		sv.MinorVersion,
		sv.EffectiveStartDate,
		sv.EffectiveEndDate
    FROM 
		[common].[vw_entityHeadDetail] ehD
		INNER JOIN common.EntityHead eh ON eh.EntityID = ehD.EntityId
		INNER JOIN [deb].[StandardVersion] sv ON ehD.[EntityID] = sv.[EntityID]
		INNER JOIN [deb].[Standard] s ON s.Id = sv.StandardId
            
GO
");
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"/****** Object:  View [deb].[vw_StandardVersionDetail]    Script Date: 09/12/2025 10:15:03 ******/
DROP VIEW IF EXISTS [deb].[vw_StandardVersionDetail]
GO

/****** Object:  View [deb].[vw_StandardVersionDetail]    Script Date: 09/12/2025 10:15:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [deb].[vw_StandardVersionDetail]
AS
    SELECT 
	    eh.EntityId,
		eh.EntityTypeTitle,
		eh.SerialNumber,
		s.Id [StandardId],
		s.Title [StandardTitle],
		sv.Delimiter,
		eh.Title,
		eh.CreatedDate,
		eh.CreatedByPostTitle,
		eh.OwnedByPostTitle,
		eh.LastModifiedByPostTitle,
		eh.LastModifiedDate,
		sv.MajorVersion,
		sv.MinorVersion,
		sv.EffectiveStartDate,
		sv.EffectiveEndDate
    FROM 
		[common].[vw_entityHeadDetail] eh
		INNER JOIN [deb].[StandardVersion] sv ON eh.[EntityID] = sv.[EntityID]
		INNER JOIN [deb].[Standard] s ON s.Id = sv.StandardId
            
GO
");
		}
    }
}
