using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ApplyCascadeDeletesToComplianceTreeNodeSummaries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplianceTreeNodeSummary_ComplianceTreeNode_ComplianceTreeNodeID",
                schema: "compliance",
                table: "ComplianceTreeNodeSummary");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplianceTreeNodeSummary_ComplianceTreeNode_ComplianceTreeNodeID",
                schema: "compliance",
                table: "ComplianceTreeNodeSummary",
                column: "ComplianceTreeNodeID",
                principalSchema: "compliance",
                principalTable: "ComplianceTreeNode",
                principalColumn: "ComplianceTreeNodeID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplianceTreeNodeSummary_ComplianceTreeNode_ComplianceTreeNodeID",
                schema: "compliance",
                table: "ComplianceTreeNodeSummary");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplianceTreeNodeSummary_ComplianceTreeNode_ComplianceTreeNodeID",
                schema: "compliance",
                table: "ComplianceTreeNodeSummary",
                column: "ComplianceTreeNodeID",
                principalSchema: "compliance",
                principalTable: "ComplianceTreeNode",
                principalColumn: "ComplianceTreeNodeID",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
