using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetValueColumnOnModuleSettingToBeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "ModuleSetting",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "ModuleSetting",
                columns: new[] { "ModuleId", "Name", "Value", "TypeId", "IsNullable", "Description", "IsCustomerSet" },
                values: new object[] { new Guid("01F76FF6-80D4-4234-AC46-BB349FCB1A7D"), "DefaultOwnerRoleIds:Statement of Compliance", "C7E4E649-1EFA-421A-813B-4B9699A45947", 7, true, "List of possible default owner role IDs", false }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DELETE FROM dbo.ModuleSetting WHERE ModuleId = '01F76FF6-80D4-4234-AC46-BB349FCB1A7D' AND Name ='DefaultOwnerRoleIds:Statement of Compliance'"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "ModuleSetting",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
