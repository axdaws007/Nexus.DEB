using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCountColumnsToComplianceTreeNode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalRequirementCount",
                schema: "compliance",
                table: "ComplianceTreeNode",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalSectionCount",
                schema: "compliance",
                table: "ComplianceTreeNode",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalRequirementCount",
                schema: "compliance",
                table: "ComplianceTreeNode");

            migrationBuilder.DropColumn(
                name: "TotalSectionCount",
                schema: "compliance",
                table: "ComplianceTreeNode");
        }
    }
}
