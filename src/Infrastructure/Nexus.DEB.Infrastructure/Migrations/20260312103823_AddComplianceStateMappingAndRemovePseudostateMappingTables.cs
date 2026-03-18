using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddComplianceStateMappingAndRemovePseudostateMappingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PseudostateMapping",
                schema: "compliance");

            migrationBuilder.AddColumn<int>(
                name: "ActivityId",
                schema: "compliance",
                table: "ComplianceTreeNode",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "compliance",
                table: "ComplianceTreeNode",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ComplianceStateMapping",
                schema: "compliance",
                columns: table => new
                {
                    ComplianceStateMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityID = table.Column<int>(type: "int", nullable: false),
                    ActivityTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StatusID = table.Column<int>(type: "int", nullable: false),
                    StatusTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ComplianceStateID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceStateMapping", x => x.ComplianceStateMappingID);
                    table.ForeignKey(
                        name: "FK_ComplianceStateMapping_ComplianceState_ComplianceStateID",
                        column: x => x.ComplianceStateID,
                        principalSchema: "compliance",
                        principalTable: "ComplianceState",
                        principalColumn: "ComplianceStateID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceStateMapping_ComplianceStateID",
                schema: "compliance",
                table: "ComplianceStateMapping",
                column: "ComplianceStateID");

            migrationBuilder.CreateIndex(
                name: "UQ_ComplianceStateMapping",
                schema: "compliance",
                table: "ComplianceStateMapping",
                columns: new[] { "WorkflowID", "ActivityID", "StatusID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplianceStateMapping",
                schema: "compliance");

            migrationBuilder.DropColumn(
                name: "ActivityId",
                schema: "compliance",
                table: "ComplianceTreeNode");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "compliance",
                table: "ComplianceTreeNode");

            migrationBuilder.CreateTable(
                name: "PseudostateMapping",
                schema: "compliance",
                columns: table => new
                {
                    PseudostateMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplianceStateID = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PseudoStateID = table.Column<int>(type: "int", nullable: false),
                    PseudoStateTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PseudostateMapping", x => x.PseudostateMappingID);
                    table.ForeignKey(
                        name: "FK_PseudostateMapping_ComplianceState_ComplianceStateID",
                        column: x => x.ComplianceStateID,
                        principalSchema: "compliance",
                        principalTable: "ComplianceState",
                        principalColumn: "ComplianceStateID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PseudostateMapping_ComplianceStateID",
                schema: "compliance",
                table: "PseudostateMapping",
                column: "ComplianceStateID");

            migrationBuilder.CreateIndex(
                name: "UQ_PseudostateMapping",
                schema: "compliance",
                table: "PseudostateMapping",
                columns: new[] { "EntityType", "PseudoStateID" },
                unique: true);
        }
    }
}
