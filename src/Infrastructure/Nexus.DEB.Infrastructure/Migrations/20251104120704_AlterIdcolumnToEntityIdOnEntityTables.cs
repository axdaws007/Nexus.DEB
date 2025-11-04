using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlterIdcolumnToEntityIdOnEntityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "Id",
                schema: "deb",
                table: "Requirement",
                newName: "EntityID");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "deb",
                table: "Scope",
                newName: "EntityID");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "deb",
                table: "StandardVersion",
                newName: "EntityID");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "deb",
                table: "Statement",
                newName: "EntityID");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "deb",
                table: "Task",
                newName: "EntityID");

            migrationBuilder.AddForeignKey(
                name: "FK_Requirement_EntityHead_EntityId",
                schema: "deb",
                table: "Requirement",
                column: "EntityId",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Scope_EntityHead_EntityId",
                schema: "deb",
                table: "Scope",
                column: "EntityId",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StandardVersion_EntityHead_EntityId",
                schema: "deb",
                table: "StandardVersion",
                column: "EntityId",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Statement_EntityHead_EntityId",
                schema: "deb",
                table: "Statement",
                column: "EntityId",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_EntityHead_EntityId",
                schema: "deb",
                table: "Task",
                column: "EntityId",
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
                schema: "deb",
                table: "Requirement",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "EntityID",
                schema: "deb",
                table: "Scope",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "EntityID",
                schema: "deb",
                table: "StandardVersion",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "EntityID",
                schema: "deb",
                table: "Statement",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "EntityID",
                schema: "deb",
                table: "Task",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requirement_EntityHead_Id",
                schema: "deb",
                table: "Requirement",
                column: "Id",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Scope_EntityHead_Id",
                schema: "deb",
                table: "Scope",
                column: "Id",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StandardVersion_EntityHead_Id",
                schema: "deb",
                table: "StandardVersion",
                column: "Id",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Statement_EntityHead_Id",
                schema: "deb",
                table: "Statement",
                column: "Id",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_EntityHead_Id",
                schema: "deb",
                table: "Task",
                column: "Id",
                principalSchema: "common",
                principalTable: "EntityHead",
                principalColumn: "EntityId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
