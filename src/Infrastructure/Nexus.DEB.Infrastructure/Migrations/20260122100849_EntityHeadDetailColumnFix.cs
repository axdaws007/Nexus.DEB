using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EntityHeadDetailColumnFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
/****** Object:  View [common].[vw_EntityHeadDetail]    Script Date: 22/01/2026 09:57:32 ******/
DROP VIEW IF EXISTS [common].[vw_EntityHeadDetail]
GO

/****** Object:  View [common].[vw_EntityHeadDetail]    Script Date: 22/01/2026 09:57:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


                CREATE VIEW [common].[vw_EntityHeadDetail]
                AS
                SELECT 
                    eh.[EntityId],
	                eh.[Title],
	                eh.[CreatedDate],
					eh.[LastModifiedDate],
	                vp_cr.[Title] AS [CreatedByPostTitle],
	                vp_lm.[Title] AS [LastModifiedByPostTitle],
	                vp_ow.[Title] AS [OwnedByPostTitle],
                    eh.SerialNumber,
					eh.EntityTypeTitle
                FROM [common].[EntityHead] eh
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
/****** Object:  View [common].[vw_EntityHeadDetail]    Script Date: 22/01/2026 09:57:32 ******/
DROP VIEW IF EXISTS [common].[vw_EntityHeadDetail]
GO

/****** Object:  View [common].[vw_EntityHeadDetail]    Script Date: 22/01/2026 09:57:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


                CREATE VIEW [common].[vw_EntityHeadDetail]
                AS
                SELECT 
                    eh.[EntityId],
	                eh.[Title],
	                eh.[CreatedDate],
					eh.[LastModifiedDate],
	                vp_cr.[Title] AS [CreatedByPostTitle],
	                vp_lm.[Title] AS [LastModifiedByPostTitle],
	                vp_cr.[Title] AS [OwnedByPostTitle],
                    eh.SerialNumber,
					eh.EntityTypeTitle
                FROM [common].[EntityHead] eh
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_cr ON eh.[CreatedById] = vp_cr.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_lm ON eh.[LastModifiedById] = vp_lm.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_ow ON eh.[OwnedById] = vp_ow.[ID]
            
GO
");
		}
    }
}
