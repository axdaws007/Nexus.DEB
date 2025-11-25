using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedStatementTextFromStatementTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatementText",
                schema: "deb",
                table: "Statement");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatementText",
                schema: "deb",
                table: "Statement",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
