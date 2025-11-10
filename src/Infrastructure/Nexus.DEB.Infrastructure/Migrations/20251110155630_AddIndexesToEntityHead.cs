using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesToEntityHead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EntityTypeTitle",
                schema: "common",
                table: "EntityHead",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_EntityHead_CreatedDate",
                schema: "common",
                table: "EntityHead",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_EntityHead_EntityTypeTitle",
                schema: "common",
                table: "EntityHead",
                column: "EntityTypeTitle");

            migrationBuilder.CreateIndex(
                name: "IX_EntityHead_LastModifiedDate",
                schema: "common",
                table: "EntityHead",
                column: "LastModifiedDate");

            migrationBuilder.CreateIndex(
                name: "IX_EntityHead_OwnedById",
                schema: "common",
                table: "EntityHead",
                column: "OwnedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EntityHead_CreatedDate",
                schema: "common",
                table: "EntityHead");

            migrationBuilder.DropIndex(
                name: "IX_EntityHead_EntityTypeTitle",
                schema: "common",
                table: "EntityHead");

            migrationBuilder.DropIndex(
                name: "IX_EntityHead_LastModifiedDate",
                schema: "common",
                table: "EntityHead");

            migrationBuilder.DropIndex(
                name: "IX_EntityHead_OwnedById",
                schema: "common",
                table: "EntityHead");

            migrationBuilder.AlterColumn<string>(
                name: "EntityTypeTitle",
                schema: "common",
                table: "EntityHead",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
