using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedChangeTrackingForUpVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			#region Section_ChangeTracking Trigger
			migrationBuilder.Sql(@"
/****** Object:  Trigger [Section_ChangeTracking]    Script Date: 18/03/2026 16:14:40 ******/
DROP TRIGGER IF EXISTS [deb].[Section_ChangeTracking]
GO

/****** Object:  Trigger [deb].[Section_ChangeTracking]    Script Date: 18/03/2026 16:14:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TRIGGER [deb].[Section_ChangeTracking]
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
            @sql NVARCHAR(MAX),
			@IgnoreAudit BIT

	DECLARE @MovedSectionId UNIQUEIDENTIFIER
	SELECT @MovedSectionId = TRY_CONVERT(UNIQUEIDENTIFIER, SESSION_CONTEXT(N'MovedSectionId'))

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
	
	SELECT @IgnoreAudit = TRY_CONVERT(BIT, SESSION_CONTEXT(N'IgnoreAudit'))
	IF @IgnoreAudit IS NULL
		SET @IgnoreAudit = 0

	IF @IgnoreAudit = 0
	BEGIN
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

			-- ══════════════════════════════════════════════════════════════
			-- DELETE: Summary row per removed section
			-- ══════════════════════════════════════════════════════════════
			IF @Type = 'D'
			BEGIN
				INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
				SELECT @ChangeRecordId,
					   'Section',
					   'Section Removed',
					   RTRIM(ISNULL(d.Reference, '') + ' ' + ISNULL(d.Title, '')),
					   NULL,
					   0
				FROM #del d
				WHERE d.StandardVersionId = @StandardVersionId
			END

			-- ══════════════════════════════════════════════════════════════
			-- INSERT / UPDATE: Field-by-field loop
			-- ══════════════════════════════════════════════════════════════
			ELSE
			BEGIN
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

					-- Check COLUMNS_UPDATED() for UPDATE, or always process for INSERT
					IF (SUBSTRING(COLUMNS_UPDATED(), (@field - 1) / 8 + 1, 1)) & POWER(2, (@field - 1) % 8) = POWER(2, (@field - 1) % 8)
						OR @Type = 'I'
					BEGIN
						SELECT @isFieldExcluded = dbo.Audit_IsFieldExcluded(@TableName, @fieldname)

						IF @isFieldExcluded = 0
						BEGIN
							IF @fieldname = 'Ordinal'
							BEGIN
								SET @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
									+ ' SELECT ' + CAST(@ChangeRecordId AS NVARCHAR(30))
									+ ',''Ordinal'''
									+ ', ''Ordinal ('' + ISNULL(RTRIM(ISNULL(COALESCE(i.Reference, d.Reference), '''') + '' '' + ISNULL(COALESCE(i.Title, d.Title), '''')), ''Unknown'') + '')'''
									+ ', convert(varchar(max), d.Ordinal)'
									+ ', convert(varchar(max), i.Ordinal)'
									+ ', 0'
									+ ' FROM #ins i FULL OUTER JOIN #del d ON i.Id = d.Id'
									+ ' WHERE ISNULL(i.StandardVersionId, d.StandardVersionId) = ''' + CAST(@StandardVersionId AS NVARCHAR(36)) + ''''
									+ ' AND (i.Ordinal <> d.Ordinal OR (i.Ordinal IS NULL AND d.Ordinal IS NOT NULL) OR (i.Ordinal IS NOT NULL AND d.Ordinal IS NULL))'

								IF @MovedSectionId IS NOT NULL
									SET @sql = @sql + ' AND COALESCE(i.Id, d.Id) = ''' + CAST(@MovedSectionId AS NVARCHAR(36)) + ''''
							END

							ELSE

							BEGIN
								-- Build dynamic INSERT into ChangeRecordItem
								SET @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
									+ ' SELECT ' + CAST(@ChangeRecordId AS NVARCHAR(30))
									+ ',''' + @fieldname + ''''
									+ ',''' + dbo.Audit_GetFieldDescription(@TableName, @fieldname) + ''''

								IF CAST(@fieldType AS VARCHAR(128)) = 'bit'
								BEGIN
									SET @sql = @sql
										+ ', CASE WHEN d.' + @fieldname + ' IS NULL THEN NULL WHEN d.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
										+ ', CASE WHEN i.' + @fieldname + ' IS NULL THEN NULL WHEN i.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
								END
								ELSE
								BEGIN
									SET @sql = @sql
										+ ', CASE WHEN d.' + @fieldname + ' IS NULL THEN NULL'
										+   ' WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), d.' + @fieldname + ')) IS NOT NULL'
										+   ' THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), d.' + @fieldname + '))'
										+   ' ELSE convert(varchar(max), d.' + @fieldname + ') END'
										+ ', CASE WHEN i.' + @fieldname + ' IS NULL THEN NULL'
										+   ' WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), i.' + @fieldname + ')) IS NOT NULL'
										+   ' THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), i.' + @fieldname + '))'
										+   ' ELSE convert(varchar(max), i.' + @fieldname + ') END'
								END

								SET @sql = @sql + ', 0'
									+ ' FROM #ins i FULL OUTER JOIN #del d ON i.Id = d.Id'
									+ ' WHERE ISNULL(i.StandardVersionId, d.StandardVersionId) = ''' + CAST(@StandardVersionId AS NVARCHAR(36)) + ''''
									+ ' AND (i.' + @fieldname + ' <> d.' + @fieldname
									+ '  OR (i.' + @fieldname + ' IS NULL AND d.' + @fieldname + ' IS NOT NULL)'
									+ '  OR (i.' + @fieldname + ' IS NOT NULL AND d.' + @fieldname + ' IS NULL))'

							END

							EXEC (@sql)
						END
					END
				END  -- field loop
			END  -- ELSE (INSERT/UPDATE)

			FETCH NEXT FROM versionCursor INTO @StandardVersionId
		END  -- version cursor

		CLOSE versionCursor
		DEALLOCATE versionCursor
	END
END
GO

ALTER TABLE [deb].[Section] ENABLE TRIGGER [Section_ChangeTracking]
GO
");
			#endregion Section_ChangeTracking Trigger

			#region SectionRequirement_ChangeTracking Trigger
			migrationBuilder.Sql(@"
/****** Object:  Trigger [SectionRequirement_ChangeTracking]    Script Date: 18/03/2026 16:21:20 ******/
DROP TRIGGER IF EXISTS [deb].[SectionRequirement_ChangeTracking]
GO

/****** Object:  Trigger [deb].[SectionRequirement_ChangeTracking]    Script Date: 18/03/2026 16:21:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE   TRIGGER [deb].[SectionRequirement_ChangeTracking]
    ON [deb].[SectionRequirement]
    AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Early exit if no rows affected
    IF NOT EXISTS (SELECT 1 FROM INSERTED) AND NOT EXISTS (SELECT 1 FROM DELETED)
        RETURN;

    DECLARE @TableName VARCHAR(128) = 'SectionRequirement',
            @Type CHAR(1),
            @Comments NVARCHAR(MAX),
            @UserName NVARCHAR(128),
            @ChangeEventId UNIQUEIDENTIFIER,
            @ChangeRecordId INT,
            @StandardVersionId UNIQUEIDENTIFIER,
            @MovedRequirementId UNIQUEIDENTIFIER,
            @MovedRequirementOldSectionId UNIQUEIDENTIFIER,
            @MovedRequirementOldOrdinal INT,
            @SuppressOrdinalAudit BIT = 0,
            @field INT,
            @maxfield INT,
            @fieldname VARCHAR(128),
            @fieldType NVARCHAR(100),
            @isFieldExcluded BIT,
            @sql NVARCHAR(MAX),
			@IgnoreAudit BIT

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

    -- Populate temp tables for dynamic SQL
    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED

	SELECT @IgnoreAudit = TRY_CONVERT(BIT, SESSION_CONTEXT(N'IgnoreAudit'))
	IF @IgnoreAudit IS NULL
		SET @IgnoreAudit = 0

	IF @IgnoreAudit = 0
	BEGIN
		-- Session context: EventId
		SELECT @ChangeEventId = TRY_CONVERT(UNIQUEIDENTIFIER, SESSION_CONTEXT(N'EventId'))
		IF @ChangeEventId IS NULL
			SET @ChangeEventId = NEWID()

		-- Session context: MovedRequirementId (set during move operations)
		SELECT @MovedRequirementId = TRY_CONVERT(UNIQUEIDENTIFIER, SESSION_CONTEXT(N'MovedRequirementId'))

		-- Session context: Old section/ordinal for cross-section moves
		SELECT @MovedRequirementOldSectionId = TRY_CONVERT(UNIQUEIDENTIFIER, SESSION_CONTEXT(N'MovedRequirementOldSectionId'))
		SELECT @MovedRequirementOldOrdinal = TRY_CONVERT(INT, SESSION_CONTEXT(N'MovedRequirementOldOrdinal'))

		-- ── Cross-section move: skip the DELETE firing entirely ──
		-- The INSERT firing will handle both the old and new values.
		IF @Type = 'D' AND @MovedRequirementId IS NOT NULL AND @MovedRequirementOldSectionId IS NOT NULL
			RETURN;

		-- Join condition: for within-section moves, join on full composite PK.
		DECLARE @PKJoinCondition NVARCHAR(200)

		IF @MovedRequirementId IS NOT NULL
			SET @PKJoinCondition = ' ON i.RequirementID = d.RequirementID'
		ELSE
			SET @PKJoinCondition = ' ON i.SectionID = d.SectionID AND i.RequirementID = d.RequirementID'

		-- Session context: SuppressOrdinalAudit (set during add/remove operations)
		DECLARE @SuppressOrdinalAuditStr NVARCHAR(10)
		SELECT @SuppressOrdinalAuditStr = TRY_CONVERT(NVARCHAR(10), SESSION_CONTEXT(N'SuppressOrdinalAudit'))
		IF LOWER(@SuppressOrdinalAuditStr) = 'true'
			SET @SuppressOrdinalAudit = 1

		-- Resolve distinct StandardVersionIds via Section
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

			-- Re-use an existing ChangeRecord for this entity + event if one already exists
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

			-- ══════════════════════════════════════════════════════════════
			-- PATH 1: CROSS-SECTION MOVE (INSERT firing with old values from session context)
			-- ══════════════════════════════════════════════════════════════
			IF @Type = 'I' AND @MovedRequirementId IS NOT NULL AND @MovedRequirementOldSectionId IS NOT NULL
			BEGIN
				DECLARE @NewSectionId UNIQUEIDENTIFIER,
						@NewOrdinal INT,
						@OldSectionFriendly NVARCHAR(500),
						@NewSectionFriendly NVARCHAR(500),
						@RequirementFriendly NVARCHAR(500)

				-- Get the new values from the inserted row
				SELECT @NewSectionId = i.SectionID,
					   @NewOrdinal = i.Ordinal
				FROM #ins i
				WHERE i.RequirementID = @MovedRequirementId

				-- Resolve friendly names
				SELECT @OldSectionFriendly = RTRIM(ISNULL(Reference, '') + ' ' + ISNULL(Title, ''))
				FROM deb.Section
				WHERE Id = @MovedRequirementOldSectionId

				SELECT @NewSectionFriendly = RTRIM(ISNULL(Reference, '') + ' ' + ISNULL(Title, ''))
				FROM deb.Section
				WHERE Id = @NewSectionId

				SELECT @RequirementFriendly = RTRIM(ISNULL(eh.SerialNumber, '') + ' ' + ISNULL(eh.Title, ''))
				FROM common.EntityHead eh
				WHERE eh.EntityID = @MovedRequirementId

				-- SectionID change
				INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
				VALUES (@ChangeRecordId, 'SectionID',
						dbo.Audit_GetFieldDescription(@TableName, 'SectionID'),
						@OldSectionFriendly, @NewSectionFriendly, 0)

				-- Ordinal change (with enriched FriendlyFieldName)
				INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
				VALUES (@ChangeRecordId, 'Ordinal',
						'Ordinal (' + ISNULL(@RequirementFriendly, 'Unknown') + ')',
						CAST(@MovedRequirementOldOrdinal AS VARCHAR(10)),
						CAST(@NewOrdinal AS VARCHAR(10)), 0)
			END

			-- ══════════════════════════════════════════════════════════════
			-- PATH 2: ADD/REMOVE (SuppressOrdinalAudit is set)
			-- Summary rows: one per requirement, showing the action clearly
			-- ══════════════════════════════════════════════════════════════
			ELSE IF @SuppressOrdinalAudit = 1 AND @Type IN ('I', 'D')
			BEGIN
				IF @Type = 'I'
				BEGIN
					-- Requirements added: one row per requirement
					INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
					SELECT @ChangeRecordId,
						   'SectionRequirement',
						   'Requirement Added',
						   NULL,
						   RTRIM(ISNULL(eh.SerialNumber, '') + ' ' + ISNULL(eh.Title, ''))
							   + ' (Section: ' + RTRIM(ISNULL(sec.Reference, '') + ' ' + ISNULL(sec.Title, '')) + ')',
						   0
					FROM #ins i
					INNER JOIN deb.Section sec ON sec.Id = i.SectionID
					INNER JOIN common.EntityHead eh ON eh.EntityID = i.RequirementID
					WHERE sec.StandardVersionId = @StandardVersionId
				END
				ELSE  -- @Type = 'D'
				BEGIN
					-- Requirements removed: one row per requirement
					INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
					SELECT @ChangeRecordId,
						   'SectionRequirement',
						   'Requirement Removed',
						   RTRIM(ISNULL(eh.SerialNumber, '') + ' ' + ISNULL(eh.Title, ''))
							   + ' (Section: ' + RTRIM(ISNULL(sec.Reference, '') + ' ' + ISNULL(sec.Title, '')) + ')',
						   NULL,
						   0
					FROM #del d
					INNER JOIN deb.Section sec ON sec.Id = d.SectionID
					INNER JOIN common.EntityHead eh ON eh.EntityID = d.RequirementID
					WHERE sec.StandardVersionId = @StandardVersionId
				END
			END

			-- ══════════════════════════════════════════════════════════════
			-- PATH 3: STANDARD (field-by-field loop for within-section moves,
			-- normal updates, and ad-hoc SQL fallback)
			-- ══════════════════════════════════════════════════════════════
			ELSE
			BEGIN
				-- Determine max column id (only needed for field loop path)
				SELECT @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID('deb.SectionRequirement'), COLUMN_NAME, 'ColumnID'))
				FROM INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_SCHEMA = 'deb' AND TABLE_NAME = @TableName

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
							-- ── Special handling for Ordinal ──
							IF @fieldname = 'Ordinal'
							BEGIN
								-- Skip entirely if suppressed (shouldn't reach here, but safety net)
								IF @SuppressOrdinalAudit = 0
								BEGIN
									SET @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
										+ ' SELECT ' + CAST(@ChangeRecordId AS NVARCHAR(30))
										+ ', ''Ordinal'''
										+ ', ''Ordinal ('' + RTRIM(ISNULL(eh.SerialNumber, '''') + '' '' + ISNULL(eh.Title, '''')) + '')'''
										+ ', convert(varchar(max), d.Ordinal)'
										+ ', convert(varchar(max), i.Ordinal)'
										+ ', 0'
										+ ' FROM #ins i FULL OUTER JOIN #del d' + @PKJoinCondition
										+ ' INNER JOIN deb.Section sec ON sec.Id = COALESCE(i.SectionID, d.SectionID)'
										+ ' LEFT JOIN common.EntityHead eh ON eh.EntityID = COALESCE(i.RequirementID, d.RequirementID)'
										+ ' WHERE sec.StandardVersionId = ''' + CAST(@StandardVersionId AS NVARCHAR(36)) + ''''
										+ ' AND (i.Ordinal <> d.Ordinal'
										+ '  OR (i.Ordinal IS NULL AND d.Ordinal IS NOT NULL)'
										+ '  OR (i.Ordinal IS NOT NULL AND d.Ordinal IS NULL))'

									-- Filter to only the moved requirement when set
									IF @MovedRequirementId IS NOT NULL
										SET @sql = @sql + ' AND COALESCE(i.RequirementID, d.RequirementID) = ''' + CAST(@MovedRequirementId AS NVARCHAR(36)) + ''''

									EXEC (@sql)
								END
							END
							-- ── Standard field handling ──
							ELSE
							BEGIN
								SET @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
									+ ' SELECT ' + CAST(@ChangeRecordId AS NVARCHAR(30))
									+ ',''' + @fieldname + ''''
									+ ',''' + dbo.Audit_GetFieldDescription(@TableName, @fieldname) + ''''

								IF CAST(@fieldType AS VARCHAR(128)) = 'bit'
								BEGIN
									SET @sql = @sql
										+ ', CASE WHEN d.' + @fieldname + ' IS NULL THEN NULL WHEN d.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
										+ ', CASE WHEN i.' + @fieldname + ' IS NULL THEN NULL WHEN i.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
								END
								ELSE
								BEGIN
									SET @sql = @sql
										+ ', CASE WHEN d.' + @fieldname + ' IS NULL THEN NULL'
										+   ' WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), d.' + @fieldname + ')) IS NOT NULL'
										+   ' THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), d.' + @fieldname + '))'
										+   ' ELSE convert(varchar(max), d.' + @fieldname + ') END'
										+ ', CASE WHEN i.' + @fieldname + ' IS NULL THEN NULL'
										+   ' WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), i.' + @fieldname + ')) IS NOT NULL'
										+   ' THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), i.' + @fieldname + '))'
										+   ' ELSE convert(varchar(max), i.' + @fieldname + ') END'
								END

								SET @sql = @sql + ', 0'
									+ ' FROM #ins i FULL OUTER JOIN #del d' + @PKJoinCondition
									+ ' INNER JOIN deb.Section sec ON sec.Id = COALESCE(i.SectionID, d.SectionID)'
									+ ' WHERE sec.StandardVersionId = ''' + CAST(@StandardVersionId AS NVARCHAR(36)) + ''''
									+ ' AND (i.' + @fieldname + ' <> d.' + @fieldname
									+ '  OR (i.' + @fieldname + ' IS NULL AND d.' + @fieldname + ' IS NOT NULL)'
									+ '  OR (i.' + @fieldname + ' IS NOT NULL AND d.' + @fieldname + ' IS NULL))'

								EXEC (@sql)
							END
						END
					END
				END  -- field loop
			END  -- ELSE (standard path)

			FETCH NEXT FROM versionCursor INTO @StandardVersionId
		END  -- version cursor

		CLOSE versionCursor
		DEALLOCATE versionCursor
	END
END
GO

ALTER TABLE [deb].[SectionRequirement] ENABLE TRIGGER [SectionRequirement_ChangeTracking]
GO
");
			#endregion SectionRequirement_ChangeTracking Trigger
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
			#region SectionRequirement_ChangeTracking Trigger
			migrationBuilder.Sql(@"
/****** Object:  Trigger [SectionRequirement_ChangeTracking]    Script Date: 20/03/2026 15:44:33 ******/
DROP TRIGGER IF EXISTS [deb].[SectionRequirement_ChangeTracking]
GO

/****** Object:  Trigger [deb].[SectionRequirement_ChangeTracking]    Script Date: 20/03/2026 15:44:34 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE   TRIGGER [deb].[SectionRequirement_ChangeTracking]
    ON [deb].[SectionRequirement]
    AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Early exit if no rows affected
    IF NOT EXISTS (SELECT 1 FROM INSERTED) AND NOT EXISTS (SELECT 1 FROM DELETED)
        RETURN;

    DECLARE @TableName VARCHAR(128) = 'SectionRequirement',
            @Type CHAR(1),
            @Comments NVARCHAR(MAX),
            @UserName NVARCHAR(128),
            @ChangeEventId UNIQUEIDENTIFIER,
            @ChangeRecordId INT,
            @StandardVersionId UNIQUEIDENTIFIER,
            @MovedRequirementId UNIQUEIDENTIFIER,
            @MovedRequirementOldSectionId UNIQUEIDENTIFIER,
            @MovedRequirementOldOrdinal INT,
            @SuppressOrdinalAudit BIT = 0,
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

    -- Populate temp tables for dynamic SQL
    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED

    -- Session context: EventId
    SELECT @ChangeEventId = TRY_CONVERT(UNIQUEIDENTIFIER, SESSION_CONTEXT(N'EventId'))
    IF @ChangeEventId IS NULL
        SET @ChangeEventId = NEWID()

    -- Session context: MovedRequirementId (set during move operations)
    SELECT @MovedRequirementId = TRY_CONVERT(UNIQUEIDENTIFIER, SESSION_CONTEXT(N'MovedRequirementId'))

    -- Session context: Old section/ordinal for cross-section moves
    SELECT @MovedRequirementOldSectionId = TRY_CONVERT(UNIQUEIDENTIFIER, SESSION_CONTEXT(N'MovedRequirementOldSectionId'))
    SELECT @MovedRequirementOldOrdinal = TRY_CONVERT(INT, SESSION_CONTEXT(N'MovedRequirementOldOrdinal'))

    -- ── Cross-section move: skip the DELETE firing entirely ──
    -- The INSERT firing will handle both the old and new values.
    IF @Type = 'D' AND @MovedRequirementId IS NOT NULL AND @MovedRequirementOldSectionId IS NOT NULL
        RETURN;

    -- Join condition: for within-section moves, join on full composite PK.
    DECLARE @PKJoinCondition NVARCHAR(200)

    IF @MovedRequirementId IS NOT NULL
        SET @PKJoinCondition = ' ON i.RequirementID = d.RequirementID'
    ELSE
        SET @PKJoinCondition = ' ON i.SectionID = d.SectionID AND i.RequirementID = d.RequirementID'

    -- Session context: SuppressOrdinalAudit (set during add/remove operations)
    DECLARE @SuppressOrdinalAuditStr NVARCHAR(10)
    SELECT @SuppressOrdinalAuditStr = TRY_CONVERT(NVARCHAR(10), SESSION_CONTEXT(N'SuppressOrdinalAudit'))
    IF LOWER(@SuppressOrdinalAuditStr) = 'true'
        SET @SuppressOrdinalAudit = 1

    -- Resolve distinct StandardVersionIds via Section
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

        -- Re-use an existing ChangeRecord for this entity + event if one already exists
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

        -- ══════════════════════════════════════════════════════════════
        -- PATH 1: CROSS-SECTION MOVE (INSERT firing with old values from session context)
        -- ══════════════════════════════════════════════════════════════
        IF @Type = 'I' AND @MovedRequirementId IS NOT NULL AND @MovedRequirementOldSectionId IS NOT NULL
        BEGIN
            DECLARE @NewSectionId UNIQUEIDENTIFIER,
                    @NewOrdinal INT,
                    @OldSectionFriendly NVARCHAR(500),
                    @NewSectionFriendly NVARCHAR(500),
                    @RequirementFriendly NVARCHAR(500)

            -- Get the new values from the inserted row
            SELECT @NewSectionId = i.SectionID,
                   @NewOrdinal = i.Ordinal
            FROM #ins i
            WHERE i.RequirementID = @MovedRequirementId

            -- Resolve friendly names
            SELECT @OldSectionFriendly = RTRIM(ISNULL(Reference, '') + ' ' + ISNULL(Title, ''))
            FROM deb.Section
            WHERE Id = @MovedRequirementOldSectionId

            SELECT @NewSectionFriendly = RTRIM(ISNULL(Reference, '') + ' ' + ISNULL(Title, ''))
            FROM deb.Section
            WHERE Id = @NewSectionId

            SELECT @RequirementFriendly = RTRIM(ISNULL(eh.SerialNumber, '') + ' ' + ISNULL(eh.Title, ''))
            FROM common.EntityHead eh
            WHERE eh.EntityID = @MovedRequirementId

            -- SectionID change
            INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
            VALUES (@ChangeRecordId, 'SectionID',
                    dbo.Audit_GetFieldDescription(@TableName, 'SectionID'),
                    @OldSectionFriendly, @NewSectionFriendly, 0)

            -- Ordinal change (with enriched FriendlyFieldName)
            INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
            VALUES (@ChangeRecordId, 'Ordinal',
                    'Ordinal (' + ISNULL(@RequirementFriendly, 'Unknown') + ')',
                    CAST(@MovedRequirementOldOrdinal AS VARCHAR(10)),
                    CAST(@NewOrdinal AS VARCHAR(10)), 0)
        END

        -- ══════════════════════════════════════════════════════════════
        -- PATH 2: ADD/REMOVE (SuppressOrdinalAudit is set)
        -- Summary rows: one per requirement, showing the action clearly
        -- ══════════════════════════════════════════════════════════════
        ELSE IF @SuppressOrdinalAudit = 1 AND @Type IN ('I', 'D')
        BEGIN
            IF @Type = 'I'
            BEGIN
                -- Requirements added: one row per requirement
                INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
                SELECT @ChangeRecordId,
                       'SectionRequirement',
                       'Requirement Added',
                       NULL,
                       RTRIM(ISNULL(eh.SerialNumber, '') + ' ' + ISNULL(eh.Title, ''))
                           + ' (Section: ' + RTRIM(ISNULL(sec.Reference, '') + ' ' + ISNULL(sec.Title, '')) + ')',
                       0
                FROM #ins i
                INNER JOIN deb.Section sec ON sec.Id = i.SectionID
                INNER JOIN common.EntityHead eh ON eh.EntityID = i.RequirementID
                WHERE sec.StandardVersionId = @StandardVersionId
            END
            ELSE  -- @Type = 'D'
            BEGIN
                -- Requirements removed: one row per requirement
                INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
                SELECT @ChangeRecordId,
                       'SectionRequirement',
                       'Requirement Removed',
                       RTRIM(ISNULL(eh.SerialNumber, '') + ' ' + ISNULL(eh.Title, ''))
                           + ' (Section: ' + RTRIM(ISNULL(sec.Reference, '') + ' ' + ISNULL(sec.Title, '')) + ')',
                       NULL,
                       0
                FROM #del d
                INNER JOIN deb.Section sec ON sec.Id = d.SectionID
                INNER JOIN common.EntityHead eh ON eh.EntityID = d.RequirementID
                WHERE sec.StandardVersionId = @StandardVersionId
            END
        END

        -- ══════════════════════════════════════════════════════════════
        -- PATH 3: STANDARD (field-by-field loop for within-section moves,
        -- normal updates, and ad-hoc SQL fallback)
        -- ══════════════════════════════════════════════════════════════
        ELSE
        BEGIN
            -- Determine max column id (only needed for field loop path)
            SELECT @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID('deb.SectionRequirement'), COLUMN_NAME, 'ColumnID'))
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = 'deb' AND TABLE_NAME = @TableName

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
                        -- ── Special handling for Ordinal ──
                        IF @fieldname = 'Ordinal'
                        BEGIN
                            -- Skip entirely if suppressed (shouldn't reach here, but safety net)
                            IF @SuppressOrdinalAudit = 0
                            BEGIN
                                SET @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
                                    + ' SELECT ' + CAST(@ChangeRecordId AS NVARCHAR(30))
                                    + ', ''Ordinal'''
                                    + ', ''Ordinal ('' + RTRIM(ISNULL(eh.SerialNumber, '''') + '' '' + ISNULL(eh.Title, '''')) + '')'''
                                    + ', convert(varchar(max), d.Ordinal)'
                                    + ', convert(varchar(max), i.Ordinal)'
                                    + ', 0'
                                    + ' FROM #ins i FULL OUTER JOIN #del d' + @PKJoinCondition
                                    + ' INNER JOIN deb.Section sec ON sec.Id = COALESCE(i.SectionID, d.SectionID)'
                                    + ' LEFT JOIN common.EntityHead eh ON eh.EntityID = COALESCE(i.RequirementID, d.RequirementID)'
                                    + ' WHERE sec.StandardVersionId = ''' + CAST(@StandardVersionId AS NVARCHAR(36)) + ''''
                                    + ' AND (i.Ordinal <> d.Ordinal'
                                    + '  OR (i.Ordinal IS NULL AND d.Ordinal IS NOT NULL)'
                                    + '  OR (i.Ordinal IS NOT NULL AND d.Ordinal IS NULL))'

                                -- Filter to only the moved requirement when set
                                IF @MovedRequirementId IS NOT NULL
                                    SET @sql = @sql + ' AND COALESCE(i.RequirementID, d.RequirementID) = ''' + CAST(@MovedRequirementId AS NVARCHAR(36)) + ''''

                                EXEC (@sql)
                            END
                        END
                        -- ── Standard field handling ──
                        ELSE
                        BEGIN
                            SET @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
                                + ' SELECT ' + CAST(@ChangeRecordId AS NVARCHAR(30))
                                + ',''' + @fieldname + ''''
                                + ',''' + dbo.Audit_GetFieldDescription(@TableName, @fieldname) + ''''

                            IF CAST(@fieldType AS VARCHAR(128)) = 'bit'
                            BEGIN
                                SET @sql = @sql
                                    + ', CASE WHEN d.' + @fieldname + ' IS NULL THEN NULL WHEN d.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
                                    + ', CASE WHEN i.' + @fieldname + ' IS NULL THEN NULL WHEN i.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
                            END
                            ELSE
                            BEGIN
                                SET @sql = @sql
                                    + ', CASE WHEN d.' + @fieldname + ' IS NULL THEN NULL'
                                    +   ' WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), d.' + @fieldname + ')) IS NOT NULL'
                                    +   ' THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), d.' + @fieldname + '))'
                                    +   ' ELSE convert(varchar(max), d.' + @fieldname + ') END'
                                    + ', CASE WHEN i.' + @fieldname + ' IS NULL THEN NULL'
                                    +   ' WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), i.' + @fieldname + ')) IS NOT NULL'
                                    +   ' THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), i.' + @fieldname + '))'
                                    +   ' ELSE convert(varchar(max), i.' + @fieldname + ') END'
                            END

                            SET @sql = @sql + ', 0'
                                + ' FROM #ins i FULL OUTER JOIN #del d' + @PKJoinCondition
                                + ' INNER JOIN deb.Section sec ON sec.Id = COALESCE(i.SectionID, d.SectionID)'
                                + ' WHERE sec.StandardVersionId = ''' + CAST(@StandardVersionId AS NVARCHAR(36)) + ''''
                                + ' AND (i.' + @fieldname + ' <> d.' + @fieldname
                                + '  OR (i.' + @fieldname + ' IS NULL AND d.' + @fieldname + ' IS NOT NULL)'
                                + '  OR (i.' + @fieldname + ' IS NOT NULL AND d.' + @fieldname + ' IS NULL))'

                            EXEC (@sql)
                        END
                    END
                END
            END  -- field loop
        END  -- ELSE (standard path)

        FETCH NEXT FROM versionCursor INTO @StandardVersionId
    END  -- version cursor

    CLOSE versionCursor
    DEALLOCATE versionCursor
END            
GO

ALTER TABLE [deb].[SectionRequirement] ENABLE TRIGGER [SectionRequirement_ChangeTracking]
GO
");
			#endregion SectionRequirement_ChangeTracking Trigger

			#region Section_ChangeTracking Trigger
			migrationBuilder.Sql(@"
/****** Object:  Trigger [Section_ChangeTracking]    Script Date: 20/03/2026 15:43:50 ******/
DROP TRIGGER IF EXISTS [deb].[Section_ChangeTracking]
GO

/****** Object:  Trigger [deb].[Section_ChangeTracking]    Script Date: 20/03/2026 15:43:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE       TRIGGER [deb].[Section_ChangeTracking]
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

	DECLARE @MovedSectionId UNIQUEIDENTIFIER
	SELECT @MovedSectionId = TRY_CONVERT(UNIQUEIDENTIFIER, SESSION_CONTEXT(N'MovedSectionId'))

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

        -- ══════════════════════════════════════════════════════════════
        -- DELETE: Summary row per removed section
        -- ══════════════════════════════════════════════════════════════
        IF @Type = 'D'
        BEGIN
            INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
            SELECT @ChangeRecordId,
                   'Section',
                   'Section Removed',
                   RTRIM(ISNULL(d.Reference, '') + ' ' + ISNULL(d.Title, '')),
                   NULL,
                   0
            FROM #del d
            WHERE d.StandardVersionId = @StandardVersionId
        END

        -- ══════════════════════════════════════════════════════════════
        -- INSERT / UPDATE: Field-by-field loop
        -- ══════════════════════════════════════════════════════════════
        ELSE
        BEGIN
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

                -- Check COLUMNS_UPDATED() for UPDATE, or always process for INSERT
                IF (SUBSTRING(COLUMNS_UPDATED(), (@field - 1) / 8 + 1, 1)) & POWER(2, (@field - 1) % 8) = POWER(2, (@field - 1) % 8)
                    OR @Type = 'I'
                BEGIN
                    SELECT @isFieldExcluded = dbo.Audit_IsFieldExcluded(@TableName, @fieldname)

                    IF @isFieldExcluded = 0
                    BEGIN
                        IF @fieldname = 'Ordinal'
                        BEGIN
                            SET @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
                                + ' SELECT ' + CAST(@ChangeRecordId AS NVARCHAR(30))
                                + ',''Ordinal'''
                                + ', ''Ordinal ('' + ISNULL(RTRIM(ISNULL(COALESCE(i.Reference, d.Reference), '''') + '' '' + ISNULL(COALESCE(i.Title, d.Title), '''')), ''Unknown'') + '')'''
                                + ', convert(varchar(max), d.Ordinal)'
                                + ', convert(varchar(max), i.Ordinal)'
                                + ', 0'
                                + ' FROM #ins i FULL OUTER JOIN #del d ON i.Id = d.Id'
                                + ' WHERE ISNULL(i.StandardVersionId, d.StandardVersionId) = ''' + CAST(@StandardVersionId AS NVARCHAR(36)) + ''''
                                + ' AND (i.Ordinal <> d.Ordinal OR (i.Ordinal IS NULL AND d.Ordinal IS NOT NULL) OR (i.Ordinal IS NOT NULL AND d.Ordinal IS NULL))'

                            IF @MovedSectionId IS NOT NULL
                                SET @sql = @sql + ' AND COALESCE(i.Id, d.Id) = ''' + CAST(@MovedSectionId AS NVARCHAR(36)) + ''''
                        END

                        ELSE

                        BEGIN
                            -- Build dynamic INSERT into ChangeRecordItem
                            SET @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
                                + ' SELECT ' + CAST(@ChangeRecordId AS NVARCHAR(30))
                                + ',''' + @fieldname + ''''
                                + ',''' + dbo.Audit_GetFieldDescription(@TableName, @fieldname) + ''''

                            IF CAST(@fieldType AS VARCHAR(128)) = 'bit'
                            BEGIN
                                SET @sql = @sql
                                    + ', CASE WHEN d.' + @fieldname + ' IS NULL THEN NULL WHEN d.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
                                    + ', CASE WHEN i.' + @fieldname + ' IS NULL THEN NULL WHEN i.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
                            END
                            ELSE
                            BEGIN
                                SET @sql = @sql
                                    + ', CASE WHEN d.' + @fieldname + ' IS NULL THEN NULL'
                                    +   ' WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), d.' + @fieldname + ')) IS NOT NULL'
                                    +   ' THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), d.' + @fieldname + '))'
                                    +   ' ELSE convert(varchar(max), d.' + @fieldname + ') END'
                                    + ', CASE WHEN i.' + @fieldname + ' IS NULL THEN NULL'
                                    +   ' WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), i.' + @fieldname + ')) IS NOT NULL'
                                    +   ' THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max), i.' + @fieldname + '))'
                                    +   ' ELSE convert(varchar(max), i.' + @fieldname + ') END'
                            END

                            SET @sql = @sql + ', 0'
                                + ' FROM #ins i FULL OUTER JOIN #del d ON i.Id = d.Id'
                                + ' WHERE ISNULL(i.StandardVersionId, d.StandardVersionId) = ''' + CAST(@StandardVersionId AS NVARCHAR(36)) + ''''
                                + ' AND (i.' + @fieldname + ' <> d.' + @fieldname
                                + '  OR (i.' + @fieldname + ' IS NULL AND d.' + @fieldname + ' IS NOT NULL)'
                                + '  OR (i.' + @fieldname + ' IS NOT NULL AND d.' + @fieldname + ' IS NULL))'

                        END

                        EXEC (@sql)
                    END
                END
            END  -- field loop
        END  -- ELSE (INSERT/UPDATE)

        FETCH NEXT FROM versionCursor INTO @StandardVersionId
    END  -- version cursor

    CLOSE versionCursor
    DEALLOCATE versionCursor
END
            
GO

ALTER TABLE [deb].[Section] ENABLE TRIGGER [Section_ChangeTracking]
GO
");
			#endregion Section_ChangeTracking Trigger
		}
	}
}
