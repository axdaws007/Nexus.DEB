using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceSchemaChangesToAsyncRebuilds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ComplianceTreeNode_Tree",
                schema: "compliance",
                table: "ComplianceTreeNode");

            migrationBuilder.DropIndex(
                name: "UQ_ComplianceTreeNode_Root",
                schema: "compliance",
                table: "ComplianceTreeNode");

            migrationBuilder.DropIndex(
                name: "UQ_ComplianceTreeNode_WithParent",
                schema: "compliance",
                table: "ComplianceTreeNode");

            migrationBuilder.AddColumn<Guid>(
                name: "BuildId",
                schema: "compliance",
                table: "ComplianceTreeNode",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ComplianceTreeBuild",
                schema: "compliance",
                columns: table => new
                {
                    StandardVersionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LiveBuildId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromotedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceTreeBuild", x => new { x.StandardVersionID, x.ScopeID });
                });

            migrationBuilder.CreateTable(
                name: "ComplianceTreeRebuildRequest",
                schema: "compliance",
                columns: table => new
                {
                    StandardVersionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BuildId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceTreeRebuildRequest", x => new { x.StandardVersionID, x.ScopeID });
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceTreeNode_Tree",
                schema: "compliance",
                table: "ComplianceTreeNode",
                column: "BuildId")
                .Annotation("SqlServer:Include", new[] { "NodeType", "EntityID", "ParentNodeType", "ParentEntityID", "ComplianceStateID", "ComplianceStateLabel" });

            migrationBuilder.CreateIndex(
                name: "UQ_ComplianceTreeNode_Root",
                schema: "compliance",
                table: "ComplianceTreeNode",
                columns: new[] { "StandardVersionID", "ScopeID", "BuildId", "NodeType", "EntityID" },
                unique: true,
                filter: "[ParentEntityID] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_ComplianceTreeNode_WithParent",
                schema: "compliance",
                table: "ComplianceTreeNode",
                columns: new[] { "StandardVersionID", "ScopeID", "BuildId", "NodeType", "EntityID", "ParentComplianceTreeNodeID" },
                unique: true,
                filter: "[ParentComplianceTreeNodeID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceTreeRebuildRequest_StatusRequestedAt",
                schema: "compliance",
                table: "ComplianceTreeRebuildRequest",
                columns: new[] { "Status", "RequestedAt" });

            migrationBuilder.Sql(@"
    SELECT StandardVersionID, ScopeID, NEWID() AS NewBuildId
    INTO #TreeBuilds
    FROM (
        SELECT DISTINCT StandardVersionID, ScopeID
        FROM compliance.ComplianceTreeNode
    ) t;

    UPDATE n
    SET n.BuildId = tb.NewBuildId
    FROM compliance.ComplianceTreeNode n
    INNER JOIN #TreeBuilds tb
        ON n.StandardVersionID = tb.StandardVersionID
        AND n.ScopeID = tb.ScopeID;

    INSERT INTO compliance.ComplianceTreeBuild (StandardVersionID, ScopeID, LiveBuildId, PromotedAt)
    SELECT StandardVersionID, ScopeID, NewBuildId, SYSUTCDATETIME()
    FROM #TreeBuilds;

    DROP TABLE #TreeBuilds;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplianceTreeBuild",
                schema: "compliance");

            migrationBuilder.DropTable(
                name: "ComplianceTreeRebuildRequest",
                schema: "compliance");

            migrationBuilder.DropIndex(
                name: "IX_ComplianceTreeNode_Tree",
                schema: "compliance",
                table: "ComplianceTreeNode");

            migrationBuilder.DropIndex(
                name: "UQ_ComplianceTreeNode_Root",
                schema: "compliance",
                table: "ComplianceTreeNode");

            migrationBuilder.DropIndex(
                name: "UQ_ComplianceTreeNode_WithParent",
                schema: "compliance",
                table: "ComplianceTreeNode");

            migrationBuilder.DropColumn(
                name: "BuildId",
                schema: "compliance",
                table: "ComplianceTreeNode");

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
                columns: new[] { "StandardVersionID", "ScopeID", "NodeType", "EntityID", "ParentComplianceTreeNodeID" },
                unique: true,
                filter: "[ParentComplianceTreeNodeID] IS NOT NULL");
        }
    }
}
