using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByPostIdColumnToCommentDetailView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [common].[vw_CommentDetail]
                AS
                SELECT
	                c.[Id],
                	c.[EntityId],
	                c.[Text],
	                c.[CreatedDate],
	                c.[CreatedByUserName],
	                c.[CreatedByPostTitle],
	                gu.[UserFirstName] AS [CreatedByFirstName],
	                gu.[UserLastName] AS [CreatedByLastName],
					c.[CreatedByPostId]
                FROM [common].[Comments] c
                LEFT JOIN [common].[XDB_CIS_Group_User] gu ON c.[CreatedByUserId] = gu.EntityID AND gu.EntityType = 'u'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [common].[vw_CommentDetail]
                AS
                SELECT
	                c.[Id],
                	c.[EntityId],
	                c.[Text],
	                c.[CreatedDate],
	                c.[CreatedByUserName],
	                c.[CreatedByPostTitle],
	                gu.[UserFirstName] AS [CreatedByFirstName],
	                gu.[UserLastName] AS [CreatedByLastName]
                FROM [common].[Comments] c
                LEFT JOIN [common].[XDB_CIS_Group_User] gu ON c.[CreatedByUserId] = gu.EntityID AND gu.EntityType = 'u'
            ");
        }
    }
}
