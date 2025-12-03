using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserReferenceInChangeRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeByUserId",
                schema: "common",
                table: "ChangeRecord");

            migrationBuilder.AddColumn<string>(
                name: "ChangeByUser",
                schema: "common",
                table: "ChangeRecord",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeByUser",
                schema: "common",
                table: "ChangeRecord");

            migrationBuilder.AddColumn<Guid>(
                name: "ChangeByUserId",
                schema: "common",
                table: "ChangeRecord",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
