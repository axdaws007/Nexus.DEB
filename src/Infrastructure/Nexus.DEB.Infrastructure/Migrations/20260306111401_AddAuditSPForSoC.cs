using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditSPForSoC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [audit].[BS10008_Snapshot_StatementofCompliance]
    @EntityId        UNIQUEIDENTIFIER,
    @EntityTypeTitle NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================================================================
    -- BS10008_Snapshot_StatementofCompliance
    -- Returns a JSON snapshot of a Statement of Compliance for audit purposes.
    --
    -- Root element  : StatementofCompliance
    -- Parent data   : common.EntityHead + deb.Statement
    -- Child data    : deb.StatementRequirementScope (with Requirement & Scope
    --                   detail from common.EntityHead)
    --                 deb.Task (SerialNumber + Title only, non-removed)
    --                 common.EntityDocumentLinking (CommonEvidence links)
    -- =========================================================================

    DECLARE @InnerJson NVARCHAR(MAX);

    SELECT @InnerJson = (
        SELECT
            -- ----------------------------------------------------------------
            -- EntityHead fields (common.EntityHead)
            -- ----------------------------------------------------------------
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

            -- ----------------------------------------------------------------
            -- Statement-specific fields (deb.Statement)
            -- ----------------------------------------------------------------
            st.ReviewDate,

            -- ----------------------------------------------------------------
            -- Child: Requirement + Scope combinations
            -- (deb.StatementRequirementScope joined to common.EntityHead
            --  for both Requirement and Scope detail)
            -- ----------------------------------------------------------------
            (
                SELECT
                    srs.RequirementId,
                    reh.SerialNumber   AS RequirementSerialNumber,
                    reh.Title          AS RequirementTitle,

                    srs.ScopeId,
                    seh.SerialNumber   AS ScopeSerialNumber,
                    seh.Title          AS ScopeTitle

                FROM deb.StatementRequirementScope srs
                INNER JOIN common.EntityHead reh ON reh.EntityId = srs.RequirementId
                INNER JOIN common.EntityHead seh ON seh.EntityId = srs.ScopeId
                WHERE srs.StatementId = @EntityId
                ORDER BY reh.SerialNumber, seh.SerialNumber
                FOR JSON PATH
            ) AS RequirementScopeCombinations,

            -- ----------------------------------------------------------------
            -- Child: Tasks (SerialNumber + Title only, non-removed)
            -- ----------------------------------------------------------------
            (
                SELECT
                    teh.SerialNumber   AS SerialNumber,
                    teh.Title          AS Title

                FROM deb.Task t
                INNER JOIN common.EntityHead teh ON teh.EntityId = t.EntityId
                WHERE t.StatementId = @EntityId
                  AND teh.IsRemoved  = 0
                ORDER BY teh.SerialNumber
                FOR JSON PATH
            ) AS Tasks,

            -- ----------------------------------------------------------------
            -- Child: Common Evidence document links
            -- (common.EntityDocumentLinking where Context = 1)
            -- ----------------------------------------------------------------
            (
                SELECT
                    edl.Id,
                    edl.LibraryId,
                    edl.DocumentId

                FROM common.EntityDocumentLinking edl
                WHERE edl.EntityId = @EntityId
                  AND edl.Context  = 1   -- EntityDocumentLinkingContexts.CommonEvidence
                FOR JSON PATH
            ) AS CommonEvidences

        FROM common.EntityHead eh
        INNER JOIN deb.Statement st ON st.EntityId = eh.EntityId
        WHERE eh.EntityId        = @EntityId
          AND eh.EntityTypeTitle = @EntityTypeTitle

        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
    );

    -- Manually wrap with the root key to work around the SQL Server
    -- limitation that ROOT and WITHOUT_ARRAY_WRAPPER cannot be combined
    SELECT '{""StatementofCompliance"":' + @InnerJson + '}';

END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE [audit].[BS10008_Snapshot_StatementofCompliance]");
        }
    }
}
