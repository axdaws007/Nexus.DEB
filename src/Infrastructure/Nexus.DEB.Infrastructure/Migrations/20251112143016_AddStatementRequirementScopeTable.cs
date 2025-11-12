using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatementRequirementScopeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StatementRequirementScope",
                schema: "deb",
                columns: table => new
                {
                    StatementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequirementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementRequirementScope", x => new { x.StatementId, x.RequirementId, x.ScopeId });
                    table.ForeignKey(
                        name: "FK_StatementRequirementScope_Requirement_RequirementId",
                        column: x => x.RequirementId,
                        principalSchema: "deb",
                        principalTable: "Requirement",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_StatementRequirementScope_Scope_ScopeId",
                        column: x => x.ScopeId,
                        principalSchema: "deb",
                        principalTable: "Scope",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_StatementRequirementScope_Statement_StatementId",
                        column: x => x.StatementId,
                        principalSchema: "deb",
                        principalTable: "Statement",
                        principalColumn: "EntityId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatementRequirementScope_RequirementId",
                schema: "deb",
                table: "StatementRequirementScope",
                column: "RequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementRequirementScope_ScopeId",
                schema: "deb",
                table: "StatementRequirementScope",
                column: "ScopeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatementRequirementScope",
                schema: "deb");
        }
    }
}
