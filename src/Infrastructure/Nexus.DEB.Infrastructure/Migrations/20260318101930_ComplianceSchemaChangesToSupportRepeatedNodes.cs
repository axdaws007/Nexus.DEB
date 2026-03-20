using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceSchemaChangesToSupportRepeatedNodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_ComplianceTreeNode_WithParent",
                schema: "compliance",
                table: "ComplianceTreeNode");

            migrationBuilder.AddColumn<long>(
                name: "ParentComplianceTreeNodeID",
                schema: "compliance",
                table: "ComplianceTreeNode",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceTreeNode_ParentNodeID",
                schema: "compliance",
                table: "ComplianceTreeNode",
                column: "ParentComplianceTreeNodeID")
                .Annotation("SqlServer:Include", new[] { "ComplianceStateID", "NodeType" });

            migrationBuilder.CreateIndex(
                name: "UQ_ComplianceTreeNode_WithParent",
                schema: "compliance",
                table: "ComplianceTreeNode",
                columns: new[] { "StandardVersionID", "ScopeID", "NodeType", "EntityID", "ParentComplianceTreeNodeID" },
                unique: true,
                filter: "[ParentComplianceTreeNodeID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplianceTreeNode_ComplianceTreeNode_ParentComplianceTreeNodeID",
                schema: "compliance",
                table: "ComplianceTreeNode",
                column: "ParentComplianceTreeNodeID",
                principalSchema: "compliance",
                principalTable: "ComplianceTreeNode",
                principalColumn: "ComplianceTreeNodeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplianceTreeNode_ComplianceTreeNode_ParentComplianceTreeNodeID",
                schema: "compliance",
                table: "ComplianceTreeNode");

            migrationBuilder.DropIndex(
                name: "IX_ComplianceTreeNode_ParentNodeID",
                schema: "compliance",
                table: "ComplianceTreeNode");

            migrationBuilder.DropIndex(
                name: "UQ_ComplianceTreeNode_WithParent",
                schema: "compliance",
                table: "ComplianceTreeNode");

            migrationBuilder.DropColumn(
                name: "ParentComplianceTreeNodeID",
                schema: "compliance",
                table: "ComplianceTreeNode");

            migrationBuilder.CreateIndex(
                name: "UQ_ComplianceTreeNode_WithParent",
                schema: "compliance",
                table: "ComplianceTreeNode",
                columns: new[] { "StandardVersionID", "ScopeID", "NodeType", "EntityID", "ParentEntityID" },
                unique: true,
                filter: "[ParentEntityID] IS NOT NULL");
        }
    }
}
