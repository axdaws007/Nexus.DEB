using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditSnapshotSPForStandardVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE SCHEMA [audit]");

            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [audit].[BS10008_Snapshot_StandardVersion]
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

            -- StandardVersion-specific fields
            sv.StandardId,
            sv.VersionTitle,
            sv.Delimiter,
            sv.MajorVersion,
            sv.MinorVersion,
            sv.EffectiveStartDate,
            sv.EffectiveEndDate,

            -- Child: Sections
            (
                SELECT
                    s.Id,
                    s.Reference,
                    s.Title,
                    s.IsReferenceDisplayed,
                    s.IsTitleDisplayed,
                    s.ParentSectionId,
                    s.Ordinal,
                    s.CreatedDate,
                    s.LastModifiedDate,

                    -- Child: SectionRequirements
                    (
                        SELECT
                            sr.SectionID,
                            sr.RequirementID,
                            sr.Ordinal,
                            sr.IsEnabled,
                            sr.LastModifiedAt,
                            sr.LastModifiedBy
                        FROM deb.SectionRequirement sr
                        WHERE sr.SectionID = s.Id
                        ORDER BY sr.Ordinal
                        FOR JSON PATH
                    ) AS SectionRequirements

                FROM deb.Section s
                WHERE s.StandardVersionId = @EntityId
                ORDER BY s.Ordinal
                FOR JSON PATH
            ) AS Sections

        FROM common.EntityHead eh
        INNER JOIN deb.StandardVersion sv ON sv.EntityId = eh.EntityId
        WHERE eh.EntityId        = @EntityId
          AND eh.EntityTypeTitle = @EntityTypeTitle

        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
    );

    -- Manually wrap with the root key to work around the SQL Server
    -- limitation that ROOT and WITHOUT_ARRAY_WRAPPER cannot be combined
    SELECT '{""StandardVersion"":' + @InnerJson + '}';

END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE [audit].[BS10008_Snapshot_StandardVersion]");

            migrationBuilder.Sql(@"DROP SCHEMA [audit]");
        }
    }
}
