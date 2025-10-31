using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "common");

            migrationBuilder.EnsureSchema(
                name: "deb");

            migrationBuilder.CreateTable(
                name: "EntityHead",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OwnedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnedByGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    EntityTypeTitle = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityHead", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequirementCategory",
                schema: "deb",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequirementCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequirementType",
                schema: "deb",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequirementType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Standard",
                schema: "deb",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Standard", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskType",
                schema: "deb",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scope",
                schema: "deb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetImplementationDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scope", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scope_EntityHead_Id",
                        column: x => x.Id,
                        principalSchema: "common",
                        principalTable: "EntityHead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Requirement",
                schema: "deb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EffectiveStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsTitleDisplayed = table.Column<bool>(type: "bit", nullable: false),
                    IsReferenceDisplayed = table.Column<bool>(type: "bit", nullable: false),
                    RequirementCategoryId = table.Column<short>(type: "smallint", nullable: false),
                    RequirementTypeId = table.Column<short>(type: "smallint", nullable: false),
                    ComplianceWeighting = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requirement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requirement_EntityHead_Id",
                        column: x => x.Id,
                        principalSchema: "common",
                        principalTable: "EntityHead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Requirement_RequirementCategory_RequirementCategoryId",
                        column: x => x.RequirementCategoryId,
                        principalSchema: "deb",
                        principalTable: "RequirementCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Requirement_RequirementType_RequirementTypeId",
                        column: x => x.RequirementTypeId,
                        principalSchema: "deb",
                        principalTable: "RequirementType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StandardVersion",
                schema: "deb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EffectiveStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MajorVersion = table.Column<int>(type: "int", nullable: true),
                    MinorVersion = table.Column<int>(type: "int", nullable: true),
                    UseVersionPrefix = table.Column<bool>(type: "bit", nullable: false),
                    StandardId = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StandardVersion_EntityHead_Id",
                        column: x => x.Id,
                        principalSchema: "common",
                        principalTable: "EntityHead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StandardVersion_Standard_StandardId",
                        column: x => x.StandardId,
                        principalSchema: "deb",
                        principalTable: "Standard",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Statement",
                schema: "deb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatementText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScopeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Statement_EntityHead_Id",
                        column: x => x.Id,
                        principalSchema: "common",
                        principalTable: "EntityHead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Statement_Scope_ScopeID",
                        column: x => x.ScopeID,
                        principalSchema: "deb",
                        principalTable: "Scope",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScopeRequirement",
                schema: "deb",
                columns: table => new
                {
                    RequirementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScopeRequirement", x => new { x.RequirementId, x.ScopeId });
                    table.ForeignKey(
                        name: "FK_ScopeRequirement_Requirement_RequirementId",
                        column: x => x.RequirementId,
                        principalSchema: "deb",
                        principalTable: "Requirement",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScopeRequirement_Scope_ScopeId",
                        column: x => x.ScopeId,
                        principalSchema: "deb",
                        principalTable: "Scope",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Section",
                schema: "deb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsReferenceDisplayed = table.Column<bool>(type: "bit", nullable: false),
                    IsTitleDisplayed = table.Column<bool>(type: "bit", nullable: false),
                    ParentSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Ordinal = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StandardVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Section", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Section_Section_ParentSectionId",
                        column: x => x.ParentSectionId,
                        principalSchema: "deb",
                        principalTable: "Section",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Section_StandardVersion_StandardVersionId",
                        column: x => x.StandardVersionId,
                        principalSchema: "deb",
                        principalTable: "StandardVersion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StandardVersionRequirement",
                schema: "deb",
                columns: table => new
                {
                    RequirementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StandardVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardVersionRequirement", x => new { x.RequirementId, x.StandardVersionId });
                    table.ForeignKey(
                        name: "FK_StandardVersionRequirement_Requirement_RequirementId",
                        column: x => x.RequirementId,
                        principalSchema: "deb",
                        principalTable: "Requirement",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StandardVersionRequirement_StandardVersion_StandardVersionId",
                        column: x => x.StandardVersionId,
                        principalSchema: "deb",
                        principalTable: "StandardVersion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StatementRequirement",
                schema: "deb",
                columns: table => new
                {
                    RequirementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementRequirement", x => new { x.RequirementId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementRequirement_Requirement_RequirementId",
                        column: x => x.RequirementId,
                        principalSchema: "deb",
                        principalTable: "Requirement",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StatementRequirement_Statement_StatementId",
                        column: x => x.StatementId,
                        principalSchema: "deb",
                        principalTable: "Statement",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Task",
                schema: "deb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskTypeId = table.Column<short>(type: "smallint", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Task", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Task_EntityHead_Id",
                        column: x => x.Id,
                        principalSchema: "common",
                        principalTable: "EntityHead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Task_Statement_StatementId",
                        column: x => x.StatementId,
                        principalSchema: "deb",
                        principalTable: "Statement",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Task_TaskType_TaskTypeId",
                        column: x => x.TaskTypeId,
                        principalSchema: "deb",
                        principalTable: "TaskType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SectionRequirement",
                schema: "deb",
                columns: table => new
                {
                    SectionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequirementID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionRequirement", x => new { x.SectionID, x.RequirementID });
                    table.ForeignKey(
                        name: "FK_SectionRequirement_Requirement_RequirementID",
                        column: x => x.RequirementID,
                        principalSchema: "deb",
                        principalTable: "Requirement",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SectionRequirement_Section_SectionID",
                        column: x => x.SectionID,
                        principalSchema: "deb",
                        principalTable: "Section",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requirement_RequirementCategoryId",
                schema: "deb",
                table: "Requirement",
                column: "RequirementCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Requirement_RequirementTypeId",
                schema: "deb",
                table: "Requirement",
                column: "RequirementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ScopeRequirement_ScopeId",
                schema: "deb",
                table: "ScopeRequirement",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_Section_ParentSectionId",
                schema: "deb",
                table: "Section",
                column: "ParentSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Section_StandardVersionId",
                schema: "deb",
                table: "Section",
                column: "StandardVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionRequirement_RequirementID",
                schema: "deb",
                table: "SectionRequirement",
                column: "RequirementID");

            migrationBuilder.CreateIndex(
                name: "IX_StandardVersion_StandardId",
                schema: "deb",
                table: "StandardVersion",
                column: "StandardId");

            migrationBuilder.CreateIndex(
                name: "IX_StandardVersionRequirement_StandardVersionId",
                schema: "deb",
                table: "StandardVersionRequirement",
                column: "StandardVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Statement_ScopeID",
                schema: "deb",
                table: "Statement",
                column: "ScopeID");

            migrationBuilder.CreateIndex(
                name: "IX_StatementRequirement_StatementId",
                schema: "deb",
                table: "StatementRequirement",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_Task_StatementId",
                schema: "deb",
                table: "Task",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_Task_TaskTypeId",
                schema: "deb",
                table: "Task",
                column: "TaskTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScopeRequirement",
                schema: "deb");

            migrationBuilder.DropTable(
                name: "SectionRequirement",
                schema: "deb");

            migrationBuilder.DropTable(
                name: "StandardVersionRequirement",
                schema: "deb");

            migrationBuilder.DropTable(
                name: "StatementRequirement",
                schema: "deb");

            migrationBuilder.DropTable(
                name: "Task",
                schema: "deb");

            migrationBuilder.DropTable(
                name: "Section",
                schema: "deb");

            migrationBuilder.DropTable(
                name: "Requirement",
                schema: "deb");

            migrationBuilder.DropTable(
                name: "Statement",
                schema: "deb");

            migrationBuilder.DropTable(
                name: "TaskType",
                schema: "deb");

            migrationBuilder.DropTable(
                name: "StandardVersion",
                schema: "deb");

            migrationBuilder.DropTable(
                name: "RequirementCategory",
                schema: "deb");

            migrationBuilder.DropTable(
                name: "RequirementType",
                schema: "deb");

            migrationBuilder.DropTable(
                name: "Scope",
                schema: "deb");

            migrationBuilder.DropTable(
                name: "Standard",
                schema: "deb");

            migrationBuilder.DropTable(
                name: "EntityHead",
                schema: "common");
        }
    }
}
