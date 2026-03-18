using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalColumnsToComplianceTreeNode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NodeLabel",
                schema: "compliance",
                table: "ComplianceTreeNode",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NodeReference",
                schema: "compliance",
                table: "ComplianceTreeNode",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ordinal",
                schema: "compliance",
                table: "ComplianceTreeNode",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NodeLabel",
                schema: "compliance",
                table: "ComplianceTreeNode");

            migrationBuilder.DropColumn(
                name: "NodeReference",
                schema: "compliance",
                table: "ComplianceTreeNode");

            migrationBuilder.DropColumn(
                name: "Ordinal",
                schema: "compliance",
                table: "ComplianceTreeNode");
        }
    }
}
