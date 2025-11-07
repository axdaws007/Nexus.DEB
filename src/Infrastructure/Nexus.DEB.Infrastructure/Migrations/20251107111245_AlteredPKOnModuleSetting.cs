using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlteredPKOnModuleSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ModuleSetting",
                table: "ModuleSetting");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModuleSetting",
                table: "ModuleSetting",
                columns: new[] { "ModuleId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ModuleSetting",
                table: "ModuleSetting");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModuleSetting",
                table: "ModuleSetting",
                column: "ModuleId");
        }
    }
}
