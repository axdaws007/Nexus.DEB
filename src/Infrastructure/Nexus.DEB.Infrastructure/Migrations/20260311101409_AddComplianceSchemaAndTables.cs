using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddComplianceSchemaAndTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "compliance");

            migrationBuilder.CreateTable(
                name: "ComplianceState",
                schema: "compliance",
                columns: table => new
                {
                    ComplianceStateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Colour = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    IsTerminal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceState", x => x.ComplianceStateID);
                });

            migrationBuilder.CreateTable(
                name: "BubbleUpRule",
                schema: "compliance",
                columns: table => new
                {
                    BubbleUpRuleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentNodeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: false),
                    Quantifier = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ChildComplianceStateID = table.Column<int>(type: "int", nullable: false),
                    ResultComplianceStateID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BubbleUpRule", x => x.BubbleUpRuleID);
                    table.CheckConstraint("CK_BubbleUpRule_Quantifier", "[Quantifier] IN ('Any', 'All')");
                    table.ForeignKey(
                        name: "FK_BubbleUpRule_ComplianceState_ChildComplianceStateID",
                        column: x => x.ChildComplianceStateID,
                        principalSchema: "compliance",
                        principalTable: "ComplianceState",
                        principalColumn: "ComplianceStateID");
                    table.ForeignKey(
                        name: "FK_BubbleUpRule_ComplianceState_ResultComplianceStateID",
                        column: x => x.ResultComplianceStateID,
                        principalSchema: "compliance",
                        principalTable: "ComplianceState",
                        principalColumn: "ComplianceStateID");
                });

            migrationBuilder.CreateTable(
                name: "ComplianceTreeNode",
                schema: "compliance",
                columns: table => new
                {
                    ComplianceTreeNodeID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StandardVersionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NodeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentNodeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ParentEntityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ComplianceStateID = table.Column<int>(type: "int", nullable: true),
                    ComplianceStateLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PseudoStateID = table.Column<int>(type: "int", nullable: true),
                    LastCalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceTreeNode", x => x.ComplianceTreeNodeID);
                    table.ForeignKey(
                        name: "FK_ComplianceTreeNode_ComplianceState_ComplianceStateID",
                        column: x => x.ComplianceStateID,
                        principalSchema: "compliance",
                        principalTable: "ComplianceState",
                        principalColumn: "ComplianceStateID");
                });

            migrationBuilder.CreateTable(
                name: "NodeDefault",
                schema: "compliance",
                columns: table => new
                {
                    NodeDefaultID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NodeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Scenario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "NoChildren"),
                    DefaultComplianceStateID = table.Column<int>(type: "int", nullable: true),
                    DefaultLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeDefault", x => x.NodeDefaultID);
                    table.ForeignKey(
                        name: "FK_NodeDefault_ComplianceState_DefaultComplianceStateID",
                        column: x => x.DefaultComplianceStateID,
                        principalSchema: "compliance",
                        principalTable: "ComplianceState",
                        principalColumn: "ComplianceStateID");
                });

            migrationBuilder.CreateTable(
                name: "PseudostateMapping",
                schema: "compliance",
                columns: table => new
                {
                    PseudostateMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PseudoStateID = table.Column<int>(type: "int", nullable: false),
                    PseudoStateTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ComplianceStateID = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ComplianceTreeNodeSummary",
                schema: "compliance",
                columns: table => new
                {
                    ComplianceTreeNodeID = table.Column<long>(type: "bigint", nullable: false),
                    ChildNodeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ComplianceStateID = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceTreeNodeSummary", x => new { x.ComplianceTreeNodeID, x.ChildNodeType, x.ComplianceStateID });
                    table.ForeignKey(
                        name: "FK_ComplianceTreeNodeSummary_ComplianceState_ComplianceStateID",
                        column: x => x.ComplianceStateID,
                        principalSchema: "compliance",
                        principalTable: "ComplianceState",
                        principalColumn: "ComplianceStateID");
                    table.ForeignKey(
                        name: "FK_ComplianceTreeNodeSummary_ComplianceTreeNode_ComplianceTreeNodeID",
                        column: x => x.ComplianceTreeNodeID,
                        principalSchema: "compliance",
                        principalTable: "ComplianceTreeNode",
                        principalColumn: "ComplianceTreeNodeID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BubbleUpRule_ChildComplianceStateID",
                schema: "compliance",
                table: "BubbleUpRule",
                column: "ChildComplianceStateID");

            migrationBuilder.CreateIndex(
                name: "IX_BubbleUpRule_ResultComplianceStateID",
                schema: "compliance",
                table: "BubbleUpRule",
                column: "ResultComplianceStateID");

            migrationBuilder.CreateIndex(
                name: "UQ_BubbleUpRule_Ordinal",
                schema: "compliance",
                table: "BubbleUpRule",
                columns: new[] { "ParentNodeType", "Ordinal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ComplianceState_Name",
                schema: "compliance",
                table: "ComplianceState",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceTreeNode_Children",
                schema: "compliance",
                table: "ComplianceTreeNode",
                columns: new[] { "StandardVersionID", "ScopeID", "ParentEntityID", "ParentNodeType" })
                .Annotation("SqlServer:Include", new[] { "ComplianceStateID", "ComplianceStateLabel" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceTreeNode_ComplianceStateID",
                schema: "compliance",
                table: "ComplianceTreeNode",
                column: "ComplianceStateID");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceTreeNode_Entity",
                schema: "compliance",
                table: "ComplianceTreeNode",
                columns: new[] { "EntityID", "NodeType" })
                .Annotation("SqlServer:Include", new[] { "StandardVersionID", "ScopeID", "ParentEntityID", "ComplianceStateID" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceTreeNode_Parent",
                schema: "compliance",
                table: "ComplianceTreeNode",
                columns: new[] { "StandardVersionID", "ScopeID", "ParentNodeType", "ParentEntityID" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceTreeNode_Tree",
                schema: "compliance",
                table: "ComplianceTreeNode",
                columns: new[] { "StandardVersionID", "ScopeID" })
                .Annotation("SqlServer:Include", new[] { "NodeType", "EntityID", "ParentNodeType", "ParentEntityID", "ComplianceStateID", "ComplianceStateLabel" });

            migrationBuilder.CreateIndex(
                name: "UQ_ComplianceTreeNode_Root",
                schema: "compliance",
                table: "ComplianceTreeNode",
                columns: new[] { "StandardVersionID", "ScopeID", "NodeType", "EntityID" },
                unique: true,
                filter: "[ParentEntityID] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_ComplianceTreeNode_WithParent",
                schema: "compliance",
                table: "ComplianceTreeNode",
                columns: new[] { "StandardVersionID", "ScopeID", "NodeType", "EntityID", "ParentEntityID" },
                unique: true,
                filter: "[ParentEntityID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceTreeNodeSummary_ComplianceStateID",
                schema: "compliance",
                table: "ComplianceTreeNodeSummary",
                column: "ComplianceStateID");

            migrationBuilder.CreateIndex(
                name: "IX_NodeDefault_DefaultComplianceStateID",
                schema: "compliance",
                table: "NodeDefault",
                column: "DefaultComplianceStateID");

            migrationBuilder.CreateIndex(
                name: "UQ_NodeDefault",
                schema: "compliance",
                table: "NodeDefault",
                columns: new[] { "NodeType", "Scenario" },
                unique: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BubbleUpRule",
                schema: "compliance");

            migrationBuilder.DropTable(
                name: "ComplianceTreeNodeSummary",
                schema: "compliance");

            migrationBuilder.DropTable(
                name: "NodeDefault",
                schema: "compliance");

            migrationBuilder.DropTable(
                name: "PseudostateMapping",
                schema: "compliance");

            migrationBuilder.DropTable(
                name: "ComplianceTreeNode",
                schema: "compliance");

            migrationBuilder.DropTable(
                name: "ComplianceState",
                schema: "compliance");
        }
    }
}
