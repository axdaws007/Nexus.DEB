using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSynonymXDB_CIS_User_Post : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE SYNONYM [common].[XDB_CIS_User_Post] FOR [EDEV_Carbon_CIS].[dbo].[vwUserPost]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP SYNONYM [common].[XDB_CIS_User_Post] FOR [EDEV_Carbon_CIS].[dbo].[vwUserPost]");
        }
    }
}
