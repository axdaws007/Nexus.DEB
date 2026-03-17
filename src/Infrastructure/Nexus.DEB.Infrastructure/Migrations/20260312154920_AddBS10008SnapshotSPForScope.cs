using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBS10008SnapshotSPForScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER   PROCEDURE [audit].[BS10008_Snapshot_Scope]
    @EntityId        UNIQUEIDENTIFIER,
    @EntityTypeTitle NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @InnerJson NVARCHAR(MAX);

    SELECT @InnerJson = (
        SELECT
            -- EntityHead fields
            eh.EntityId,
            eh.EntityTypeTitle,
            eh.SerialNumber,
            eh.Title,
            eh.Description,
            eh.ModuleId,
            eh.OwnedById,
            eh.OwnedByGroupId,
            eh.CreatedById,
            eh.CreatedDate,
            eh.LastModifiedById,
            eh.LastModifiedDate,
            eh.IsRemoved,
            eh.IsArchived,

            -- Scope-specific fields
            sc.TargetImplementationDate,

            -- Child: ScopeRequirements
            (
                SELECT
                    sr.RequirementId
                FROM deb.ScopeRequirement sr
                WHERE sr.ScopeId = @EntityId
                FOR JSON PATH
            ) AS ScopeRequirements

        FROM common.EntityHead eh
        INNER JOIN deb.Scope sc ON sc.EntityId = eh.EntityId
        WHERE eh.EntityId        = @EntityId
          AND eh.EntityTypeTitle = @EntityTypeTitle

        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
    );

    -- Manually wrap with the root key to work around the SQL Server
    -- limitation that ROOT and WITHOUT_ARRAY_WRAPPER cannot be combined
    SELECT '{""Scope"":' + @InnerJson + '}';

END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE [audit].[BS10008_Snapshot_Scope]");
        }
    }
}
