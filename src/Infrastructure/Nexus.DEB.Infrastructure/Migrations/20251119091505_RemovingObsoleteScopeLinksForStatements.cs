using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovingObsoleteScopeLinksForStatements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Statement_Scope_ScopeID",
                schema: "deb",
                table: "Statement");

            migrationBuilder.DropTable(
                name: "StatementRequirement",
                schema: "deb");

            migrationBuilder.DropIndex(
                name: "IX_Statement_ScopeID",
                schema: "deb",
                table: "Statement");

            migrationBuilder.DropColumn(
                name: "ScopeID",
                schema: "deb",
                table: "Statement");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ScopeID",
                schema: "deb",
                table: "Statement",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "StatementRequirement",
                schema: "deb",
                columns: table => new
                {
                    RequirementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementRequirement", x => new { x.RequirementId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementRequirement_Requirement_RequirementId",
                        column: x => x.RequirementId,
                        principalSchema: "deb",
                        principalTable: "Requirement",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_StatementRequirement_Statement_StatementId",
                        column: x => x.StatementId,
                        principalSchema: "deb",
                        principalTable: "Statement",
                        principalColumn: "EntityId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Statement_ScopeID",
                schema: "deb",
                table: "Statement",
                column: "ScopeID");

            migrationBuilder.CreateIndex(
                name: "IX_StatementRequirement_StatementId",
                schema: "deb",
                table: "StatementRequirement",
                column: "StatementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Statement_Scope_ScopeID",
                schema: "deb",
                table: "Statement",
                column: "ScopeID",
                principalSchema: "deb",
                principalTable: "Scope",
                principalColumn: "EntityId");
        }
    }
}
