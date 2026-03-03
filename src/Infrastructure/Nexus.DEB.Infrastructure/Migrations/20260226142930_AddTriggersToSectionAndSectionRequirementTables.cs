using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTriggersToSectionAndSectionRequirementTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
EXEC sys.sp_addextendedproperty
    @name       = N'NXS_IsIgnoredForAudit',
    @value      = N'true',
    @level0type = N'SCHEMA', @level0name = N'deb',
    @level1type = N'TABLE',  @level1name = N'Section',
    @level2type = N'COLUMN', @level2name = N'CreatedDate';
            ");

            migrationBuilder.Sql(@"
EXEC sys.sp_addextendedproperty
    @name       = N'NXS_IsIgnoredForAudit',
    @value      = N'true',
    @level0type = N'SCHEMA', @level0name = N'deb',
    @level1type = N'TABLE',  @level1name = N'Section',
    @level2type = N'COLUMN', @level2name = N'LastModifiedDate';
            ");

            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER [deb].[Section_ChangeTracking]
    ON [deb].[Section]
    AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Early exit if no rows affected
    IF NOT EXISTS (SELECT 1 FROM INSERTED) AND NOT EXISTS (SELECT 1 FROM DELETED)
        RETURN;

    DECLARE @TableName VARCHAR(128) = 'Section',
            @Type CHAR(1),
            @Comments NVARCHAR(MAX),
            @UserName NVARCHAR(128),
            @ChangeEventId UNIQUEIDENTIFIER,
            @ChangeRecordId INT,
            @StandardVersionId UNIQUEIDENTIFIER,
            @field INT,
            @maxfield INT,
            @fieldname VARCHAR(128),
            @fieldType NVARCHAR(100),
            @isFieldExcluded BIT,
            @sql NVARCHAR(MAX)

    -- Determine operation type
    IF EXISTS (SELECT 1 FROM INSERTED)
        IF EXISTS (SELECT 1 FROM DELETED)
        BEGIN
            SELECT @Type = 'U'
            SELECT @Comments = 'Section updated'
        END
        ELSE
        BEGIN
            SELECT @Type = 'I'
            SELECT @Comments = 'Section created'
        END
    ELSE
    BEGIN
        SELECT @Type = 'D'
        SELECT @Comments = 'Section removed'
    END

    SELECT @UserName = dbo.Audit_GetUserContext()

    -- Populate temp tables for dynamic SQL
    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED

    -- Get EventId from session context (set by the application per SaveChanges call)
    SELECT @ChangeEventId = TRY_CONVERT(UNIQUEIDENTIFIER, SESSION_CONTEXT(N'EventId'))
    IF @ChangeEventId IS NULL
        SET @ChangeEventId = NEWID()

    -- Collect all distinct StandardVersionIds affected by this operation.
    -- UNION covers the case where a Section is moved between StandardVersions.
    DECLARE @AffectedVersions TABLE (StandardVersionId UNIQUEIDENTIFIER)

    INSERT INTO @AffectedVersions (StandardVersionId)
    SELECT DISTINCT StandardVersionId FROM INSERTED
    UNION
    SELECT DISTINCT StandardVersionId FROM DELETED

    -- Determine max column id once (used for the field loop)
    SELECT @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID('deb.Section'), COLUMN_NAME, 'ColumnID'))
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'deb' AND TABLE_NAME = @TableName

    -- ── Per-StandardVersion processing ──
    DECLARE versionCursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT StandardVersionId FROM @AffectedVersions

    OPEN versionCursor
    FETCH NEXT FROM versionCursor INTO @StandardVersionId

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @ChangeRecordId = NULL

        -- Re-use an existing ChangeRecord for this entity + event if one already
        -- exists (e.g. if the EntityHead trigger or another child trigger already
        -- created one within the same SaveChanges / EventId).
        SELECT @ChangeRecordId = Id
        FROM common.ChangeRecord
        WHERE EventId = @ChangeEventId
          AND EntityId = @StandardVersionId

        IF @ChangeRecordId IS NULL
        BEGIN
            INSERT INTO common.ChangeRecord (EntityId, ChangeDate, Comments, ChangeByUser, IsDeleted, EventId)
            VALUES (@StandardVersionId, GETDATE(), @Comments, @UserName, 0, @ChangeEventId)

            SET @ChangeRecordId = SCOPE_IDENTITY()
        END

        -- ── Field-by-field loop (mirrors EntityHead_ChangeTracking) ──
        SET @field = 0

        WHILE @field < @maxfield
        BEGIN
            -- Advance to next column id
            ;WITH nextField AS (
                SELECT MIN(COLUMNPROPERTY(OBJECT_ID('deb.' + @TableName), COLUMN_NAME, 'ColumnID')) AS ColumnID
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = 'deb'
                  AND TABLE_NAME  = @TableName
                  AND COLUMNPROPERTY(OBJECT_ID('deb.' + @TableName), COLUMN_NAME, 'ColumnID') > @field
            )
            SELECT @field     = nf.ColumnID,
                   @fieldname = sc.COLUMN_NAME,
                   @fieldType = sc.DATA_TYPE
            FROM nextField nf
            INNER JOIN INFORMATION_SCHEMA.COLUMNS sc
                ON nf.ColumnID = COLUMNPROPERTY(OBJECT_ID(sc.TABLE_SCHEMA + '.' + sc.TABLE_NAME), sc.COLUMN_NAME, 'ColumnID')
            WHERE sc.TABLE_SCHEMA = 'deb'
              AND sc.TABLE_NAME   = @TableName

            -- Check COLUMNS_UPDATED() for UPDATE, or always process for INSERT/DELETE
            IF (SUBSTRING(COLUMNS_UPDATED(), (@field - 1) / 8 + 1, 1)) & POWER(2, (@field - 1) % 8) = POWER(2, (@field - 1) % 8)
                OR @Type IN ('I', 'D')
            BEGIN
                SELECT @isFieldExcluded = dbo.Audit_IsFieldExcluded(@TableName, @fieldname)

                IF @isFieldExcluded = 0
                BEGIN
                    -- Build dynamic INSERT into ChangeRecordItem
                    SET @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
                        + ' SELECT ''' + CAST(@ChangeRecordId AS NVARCHAR(30)) + ''''
                        + ',''' + @fieldname + ''''
                        + ',''' + dbo.Audit_GetFieldDescription(@TableName, @fieldname) + ''''

                    IF CAST(@fieldType AS VARCHAR(128)) = 'bit'
                    BEGIN
                        SET @sql = @sql
                            + ', CASE WHEN d.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
                            + ', CASE WHEN i.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
                    END
                    ELSE
                    BEGIN
                        SET @sql = @sql
                            + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), d.' + @fieldname + ')) IS NOT NULL'
                            +   ' THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), d.' + @fieldname + '))'
                            +   ' ELSE convert(varchar(max), d.' + @fieldname + ') END'
                            + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), i.' + @fieldname + ')) IS NOT NULL'
                            +   ' THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), i.' + @fieldname + '))'
                            +   ' ELSE convert(varchar(max), i.' + @fieldname + ') END'
                    END

                    SET @sql = @sql + ', 0'
                        + ' FROM #ins i FULL OUTER JOIN #del d ON i.Id = d.Id'
                        + ' WHERE ISNULL(i.StandardVersionId, d.StandardVersionId) = ''' + CAST(@StandardVersionId AS NVARCHAR(36)) + ''''
                        + ' AND (i.' + @fieldname + ' <> d.' + @fieldname
                        + '  OR (i.' + @fieldname + ' IS NULL AND d.' + @fieldname + ' IS NOT NULL)'
                        + '  OR (i.' + @fieldname + ' IS NOT NULL AND d.' + @fieldname + ' IS NULL))'

                    EXEC (@sql)
                END
            END
        END  -- field loop

        FETCH NEXT FROM versionCursor INTO @StandardVersionId
    END  -- version cursor

    CLOSE versionCursor
    DEALLOCATE versionCursor
END
            ");

            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER [deb].[SectionRequirement_ChangeTracking]
    ON [deb].[SectionRequirement]
    AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Early exit if no rows affected
    IF NOT EXISTS (SELECT 1 FROM INSERTED) AND NOT EXISTS (SELECT 1 FROM DELETED)
        RETURN;

    DECLARE @TableName VARCHAR(128)         = 'SectionRequirement',
            @FriendlyTableName VARCHAR(128) = 'Section / Requirement',
            @Type CHAR(1),
            @Comments NVARCHAR(MAX),
            @UserName NVARCHAR(128),
            @ChangeEventId UNIQUEIDENTIFIER,
            @ChangeRecordId INT,
            @ChangeRecordItemId INT,
            @StandardVersionId UNIQUEIDENTIFIER,
            @ChangedFrom NVARCHAR(MAX),
            @ChangedTo NVARCHAR(MAX)

    -- Determine operation type
    IF EXISTS (SELECT 1 FROM INSERTED)
        IF EXISTS (SELECT 1 FROM DELETED)
        BEGIN
            SELECT @Type = 'U'
            SELECT @Comments = 'Section/Requirements changed'
        END
        ELSE
        BEGIN
            SELECT @Type = 'I'
            SELECT @Comments = 'Section/Requirements changed'
        END
    ELSE
    BEGIN
        SELECT @Type = 'D'
        SELECT @Comments = 'Section/Requirements changed'
    END

    SELECT @UserName = dbo.Audit_GetUserContext()

    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED

    -- Get EventId from session context
    SELECT @ChangeEventId = TRY_CONVERT(UNIQUEIDENTIFIER, SESSION_CONTEXT(N'EventId'))
    IF @ChangeEventId IS NULL
        SET @ChangeEventId = NEWID()

    -- Resolve distinct StandardVersionIds via Section.
    -- We need to join through Section to get the parent StandardVersionId.
    DECLARE @AffectedVersions TABLE (StandardVersionId UNIQUEIDENTIFIER)

    INSERT INTO @AffectedVersions (StandardVersionId)
    SELECT DISTINCT s.StandardVersionId
    FROM INSERTED i
    INNER JOIN deb.Section s ON s.Id = i.SectionID
    UNION
    SELECT DISTINCT s.StandardVersionId
    FROM DELETED d
    INNER JOIN deb.Section s ON s.Id = d.SectionID

    -- ── Per-StandardVersion processing ──
    DECLARE versionCursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT StandardVersionId FROM @AffectedVersions

    OPEN versionCursor
    FETCH NEXT FROM versionCursor INTO @StandardVersionId

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @ChangeRecordId = NULL
        SET @ChangeRecordItemId = NULL
        SET @ChangedFrom = NULL
        SET @ChangedTo = NULL

        -- Find or create ChangeRecord for this StandardVersion + EventId
        SELECT @ChangeRecordId = Id
        FROM common.ChangeRecord
        WHERE EventId = @ChangeEventId
          AND EntityId = @StandardVersionId

        IF @ChangeRecordId IS NULL
        BEGIN
            INSERT INTO common.ChangeRecord (EntityId, ChangeDate, Comments, ChangeByUser, IsDeleted, EventId)
            VALUES (@StandardVersionId, GETDATE(), @Comments, @UserName, 0, @ChangeEventId)

            SET @ChangeRecordId = SCOPE_IDENTITY()
        END

        -- Check if a ChangeRecordItem already exists for this ChangeRecord + field
        -- (possible if multiple triggers fire within the same event)
        SELECT @ChangeRecordItemId = Id
        FROM common.ChangeRecordItem
        WHERE ChangeRecordId = @ChangeRecordId
          AND FieldName = @TableName

        -- ── Build ChangedFrom (state BEFORE this operation) ──
        -- Formula: (current table state MINUS inserted rows) UNION ALL (deleted rows)
        -- This works correctly for INSERT, UPDATE, and DELETE:
        --   INSERT: deleted is empty  → current minus new rows = previous state
        --   UPDATE: replaces new values with old values = previous state
        --   DELETE: deleted has removed rows, current has them gone = previous state restored

        IF @ChangeRecordItemId IS NULL
        BEGIN
            -- First time recording this field in this event — compute ChangedFrom
            ;WITH BeforeState AS (
                -- Current rows NOT in INSERTED (i.e. rows that weren't just added/updated)
                SELECT sr.SectionID, sr.RequirementID, sr.Ordinal, sr.IsEnabled
                FROM deb.SectionRequirement sr
                INNER JOIN deb.Section s ON s.Id = sr.SectionID
                LEFT JOIN #ins i ON i.SectionID = sr.SectionID AND i.RequirementID = sr.RequirementID
                WHERE s.StandardVersionId = @StandardVersionId
                  AND i.SectionID IS NULL
                UNION ALL
                -- Plus the DELETED (old) versions of changed/removed rows
                SELECT d.SectionID, d.RequirementID, d.Ordinal, d.IsEnabled
                FROM #del d
                INNER JOIN deb.Section s ON s.Id = d.SectionID
                WHERE s.StandardVersionId = @StandardVersionId
            )
            SELECT @ChangedFrom = STRING_AGG(
                CONCAT(
                    'Section: ', ISNULL(sec.Reference, CAST(bs.SectionID AS NVARCHAR(36))),
                    ', Req: ', ISNULL(ehReq.SerialNumber, CAST(bs.RequirementID AS NVARCHAR(36))),
                    ', Ordinal: ', bs.Ordinal,
                    ', Enabled: ', CASE WHEN bs.IsEnabled = 1 THEN 'True' ELSE 'False' END
                ), '; ')
            FROM BeforeState bs
            INNER JOIN deb.Section sec ON sec.Id = bs.SectionID
            INNER JOIN common.EntityHead ehReq ON ehReq.EntityID = bs.RequirementID
        END

        -- ── Build ChangedTo (state AFTER this operation = current table) ──
        SELECT @ChangedTo = STRING_AGG(
            CONCAT(
                'Section: ', ISNULL(sec.Reference, CAST(sr.SectionID AS NVARCHAR(36))),
                ', Req: ', ISNULL(ehReq.SerialNumber, CAST(sr.RequirementID AS NVARCHAR(36))),
                ', Ordinal: ', sr.Ordinal,
                ', Enabled: ', CASE WHEN sr.IsEnabled = 1 THEN 'True' ELSE 'False' END
            ), '; ')
        FROM deb.SectionRequirement sr
        INNER JOIN deb.Section sec ON sec.Id = sr.SectionID
        INNER JOIN common.EntityHead ehReq ON ehReq.EntityID = sr.RequirementID
        WHERE sec.StandardVersionId = @StandardVersionId

        -- ── Insert or update the ChangeRecordItem ──
        IF @ChangeRecordItemId IS NULL
        BEGIN
            INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
            VALUES (@ChangeRecordId, @TableName, @FriendlyTableName, @ChangedFrom, @ChangedTo, 0)
        END
        ELSE
        BEGIN
            -- ChangeRecordItem already exists — just update ChangedTo (preserving original ChangedFrom)
            UPDATE common.ChangeRecordItem
            SET ChangedTo = @ChangedTo
            WHERE Id = @ChangeRecordItemId
        END

        FETCH NEXT FROM versionCursor INTO @StandardVersionId
    END

    CLOSE versionCursor
    DEALLOCATE versionCursor
END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER [deb].[SectionRequirement_ChangeTracking]");

            migrationBuilder.Sql(@"DROP TRIGGER [deb].[Section_ChangeTracking]");
        }
    }
}
