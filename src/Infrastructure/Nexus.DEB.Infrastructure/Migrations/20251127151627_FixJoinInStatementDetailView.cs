using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixJoinInStatementDetailView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StatementDetail]
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
	                st.[ReviewDate],
	                vp_cr.[Title] AS [CreatedBy],
	                vp_lm.[Title] AS [LastModifiedBy],
	                vp_ow.[Title] AS [OwnedBy],
					eh.OwnedById
                FROM [common].[EntityHead] eh
                INNER JOIN [deb].[Statement] st ON eh.[EntityID] = st.[EntityID]
				LEFT JOIN [deb].[StatementRequirementScope] srs ON st.[EntityID] = srs.[StatementId]
                LEFT JOIN [common].[EntityHead] ehsc ON srs.[ScopeID] = ehsc.[EntityID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_cr ON eh.[CreatedById] = vp_cr.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_lm ON eh.[LastModifiedById] = vp_lm.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_ow ON eh.[OwnedById] = vp_ow.[ID]
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StatementDetail]
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
	                st.[ReviewDate],
	                vp_cr.[Title] AS [CreatedBy],
	                vp_lm.[Title] AS [LastModifiedBy],
	                vp_ow.[Title] AS [OwnedBy],
					eh.OwnedById
                FROM [common].[EntityHead] eh
                INNER JOIN [deb].[Statement] st ON eh.[EntityID] = st.[EntityID]
				INNER JOIN [deb].[StatementRequirementScope] srs ON st.[EntityID] = srs.[StatementId]
                LEFT JOIN [common].[EntityHead] ehsc ON srs.[ScopeID] = ehsc.[EntityID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_cr ON eh.[CreatedById] = vp_cr.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_lm ON eh.[LastModifiedById] = vp_lm.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_ow ON eh.[OwnedById] = vp_ow.[ID]
            ");
        }
    }
}
