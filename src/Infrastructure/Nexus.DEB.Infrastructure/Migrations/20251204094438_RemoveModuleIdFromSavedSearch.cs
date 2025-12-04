using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveModuleIdFromSavedSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SavedSearch",
                schema: "common",
                table: "SavedSearch");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                schema: "common",
                table: "SavedSearch");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SavedSearch",
                schema: "common",
                table: "SavedSearch",
                columns: new[] { "PostId", "Name", "Context" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SavedSearch",
                schema: "common",
                table: "SavedSearch");

            migrationBuilder.AddColumn<Guid>(
                name: "ModuleId",
                schema: "common",
                table: "SavedSearch",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_SavedSearch",
                schema: "common",
                table: "SavedSearch",
                columns: new[] { "PostId", "Name", "Context", "ModuleId" });
        }
    }
}
