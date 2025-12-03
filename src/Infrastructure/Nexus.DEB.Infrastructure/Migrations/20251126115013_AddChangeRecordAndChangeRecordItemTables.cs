using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChangeRecordAndChangeRecordItemTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChangeRecord",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeRecordItem",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChangeRecordId = table.Column<int>(type: "int", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FriendlyFieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedFrom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeRecordItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeRecordItem_ChangeRecord_ChangeRecordId",
                        column: x => x.ChangeRecordId,
                        principalSchema: "common",
                        principalTable: "ChangeRecord",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRecord_EntityId",
                schema: "common",
                table: "ChangeRecord",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRecordItem_ChangeRecordId",
                schema: "common",
                table: "ChangeRecordItem",
                column: "ChangeRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChangeRecordItem",
                schema: "common");

            migrationBuilder.DropTable(
                name: "ChangeRecord",
                schema: "common");
        }
    }
}
