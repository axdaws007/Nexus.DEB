using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSerialNumberColumnToEntityHeadDetailsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [common].[vw_EntityHeadDetail]
                AS
                SELECT 
                    eh.[EntityId],
	                eh.[Title],
	                eh.[CreatedDate],
					eh.[LastModifiedDate],
	                vp_cr.[Title] AS [CreatedByPostTitle],
	                vp_lm.[Title] AS [LastModifiedByPostTitle],
	                vp_cr.[Title] AS [OwnedByPostTitle],
                    eh.SerialNumber
                FROM [common].[EntityHead] eh
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_cr ON eh.[CreatedById] = vp_cr.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_lm ON eh.[LastModifiedById] = vp_lm.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_ow ON eh.[OwnedById] = vp_ow.[ID]
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [common].[vw_EntityHeadDetail]
                AS
                SELECT 
                    eh.[EntityId],
	                eh.[Title],
	                eh.[CreatedDate],
					eh.[LastModifiedDate],
	                vp_cr.[Title] AS [CreatedByPostTitle],
	                vp_lm.[Title] AS [LastModifiedByPostTitle],
	                vp_cr.[Title] AS [OwnedByPostTitle]
                FROM [common].[EntityHead] eh
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_cr ON eh.[CreatedById] = vp_cr.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_lm ON eh.[LastModifiedById] = vp_lm.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_ow ON eh.[OwnedById] = vp_ow.[ID]
            ");
        }
    }
}
