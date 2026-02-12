using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityDocumentLinkingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntityDocumentLinking",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LibraryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Context = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityDocumentLinking", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntityDocumentLinking_EntityId_Context",
                schema: "common",
                table: "EntityDocumentLinking",
                columns: new[] { "EntityId", "Context" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityDocumentLinking_LibraryId_DocumentId",
                schema: "common",
                table: "EntityDocumentLinking",
                columns: new[] { "LibraryId", "DocumentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntityDocumentLinking",
                schema: "common");
        }
    }
}
