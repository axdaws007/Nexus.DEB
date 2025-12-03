using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventIdToChangeRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                schema: "common",
                table: "ChangeRecord",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventId",
                schema: "common",
                table: "ChangeRecord");
        }
    }
}
