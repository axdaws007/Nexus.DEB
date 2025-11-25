using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModuleInfoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuleInfo",
                columns: table => new
                {
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AssemblyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IOCName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    SchemaName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsIssueLinkable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleInfo", x => x.ModuleId);
                });

            migrationBuilder.InsertData(
                table: "ModuleInfo",
                columns: new[] { "ModuleId", "ModuleName", "AssemblyName", "IOCName", "Enabled", "SchemaName", "IsIssueLinkable" },
                values: new object[] { new Guid("01F76FF6-80D4-4234-AC46-BB349FCB1A7D"), "DEB", null, null, true, "deb", false }
            );

			migrationBuilder.AddForeignKey(
                name: "FK_ModuleSetting_ModuleInfo_ModuleId",
                table: "ModuleSetting",
                column: "ModuleId",
                principalTable: "ModuleInfo",
                principalColumn: "ModuleId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModuleSetting_ModuleInfo_ModuleId",
                table: "ModuleSetting");

            migrationBuilder.DropTable(
                name: "ModuleInfo");
        }
    }
}
