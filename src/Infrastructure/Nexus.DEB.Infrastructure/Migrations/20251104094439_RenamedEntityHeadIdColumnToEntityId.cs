using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenamedEntityHeadIdColumnToEntityId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requirement_EntityHead_Id",
                schema: "deb",
                table: "Requirement");

            migrationBuilder.DropForeignKey(
                name: "FK_Scope_EntityHead_Id",
                schema: "deb",
                table: "Scope");

            migrationBuilder.DropForeignKey(
                name: "FK_StandardVersion_EntityHead_Id",
                schema: "deb",
                table: "StandardVersion");

            migrationBuilder.DropForeignKey(
                name: "FK_Statement_EntityHead_Id",
                schema: "deb",
                table: "Statement");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_EntityHead_Id",
                schema: "deb",
                table: "Task");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "common",
                table: "EntityHead",
                newName: "EntityID");

            migrationBuilder.AddForeignKey(
                name: "FK_Requirement_EntityHead_EntityId",
                schema: "deb",
                table: "Requirement",
                column: "Id",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Scope_EntityHead_EntityId",
                schema: "deb",
                table: "Scope",
                column: "Id",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StandardVersion_EntityHead_EntityId",
                schema: "deb",
                table: "StandardVersion",
                column: "Id",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Statement_EntityHead_EntityId",
                schema: "deb",
                table: "Statement",
                column: "Id",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_EntityHead_EntityId",
                schema: "deb",
                table: "Task",
                column: "Id",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requirement_EntityHead_EntityId",
                schema: "deb",
                table: "Requirement");

            migrationBuilder.DropForeignKey(
                name: "FK_Scope_EntityHead_EntityId",
                schema: "deb",
                table: "Scope");

            migrationBuilder.DropForeignKey(
                name: "FK_StandardVersion_EntityHead_EntityId",
                schema: "deb",
                table: "StandardVersion");

            migrationBuilder.DropForeignKey(
                name: "FK_Statement_EntityHead_EntityId",
                schema: "deb",
                table: "Statement");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_EntityHead_EntityId",
                schema: "deb",
                table: "Task");

            migrationBuilder.RenameColumn(
                name: "EntityID",
                schema: "common",
                table: "EntityHead",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requirement_EntityHead_Id",
                schema: "deb",
                table: "Requirement",
                column: "Id",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Scope_EntityHead_Id",
                schema: "deb",
                table: "Scope",
                column: "Id",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StandardVersion_EntityHead_Id",
                schema: "deb",
                table: "StandardVersion",
                column: "Id",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Statement_EntityHead_Id",
                schema: "deb",
                table: "Statement",
                column: "Id",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_EntityHead_Id",
                schema: "deb",
                table: "Task",
                column: "Id",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
