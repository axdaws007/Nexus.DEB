using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePawsActivityTransition21ToAddMutTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE paws.PAWSActivityTransition
SET MUTHandler = 'ValidateActiveStandardVersions'
WHERE ActivityTransitionID = 21
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE paws.PAWSActivityTransition
SET MUTHandler = NULL
WHERE ActivityTransitionID = 21
            ");
        }
    }
}
