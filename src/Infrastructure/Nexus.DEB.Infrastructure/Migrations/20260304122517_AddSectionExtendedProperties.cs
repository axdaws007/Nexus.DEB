using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSectionExtendedProperties : Migration
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
    @level2type = N'COLUMN', @level2name = N'Id';
            ");

            migrationBuilder.Sql(@"
EXEC sys.sp_addextendedproperty 
    @name       = N'NXS_IsIgnoredForAudit',
    @value      = N'true',
    @level0type = N'SCHEMA', @level0name = N'deb',
    @level1type = N'TABLE',  @level1name = N'Section',
    @level2type = N'COLUMN', @level2name = N'StandardVersionId';
            ");

            migrationBuilder.Sql(@"
EXEC sys.sp_addextendedproperty
    @name       = N'NXS_ExternalDataSource',
    @value      = N'parentsection',
    @level0type = N'SCHEMA', @level0name = N'deb',
    @level1type = N'TABLE',  @level1name = N'Section',
    @level2type = N'COLUMN', @level2name = N'ParentSectionId';
            ");

            migrationBuilder.Sql(@"
EXEC sys.sp_addextendedproperty
    @name       = N'MS_Description',
    @value      = N'Parent Section',
    @level0type = N'SCHEMA', @level0name = N'deb',
    @level1type = N'TABLE',  @level1name = N'Section',
    @level2type = N'COLUMN', @level2name = N'ParentSectionId';
            ");

            migrationBuilder.Sql(@"
ALTER FUNCTION [dbo].[Audit_GetLookupValue]
(
	@sourceTableName varchar( 128 ),
	@sourceFieldName varchar( 128 ),
	@ID varchar( 128 )
)
RETURNS nvarchar( 128 )
AS
BEGIN
	DECLARE @fkTable varchar(100)
	DECLARE @fieldType varchar(100)
	DECLARE @schemaName varchar(100)
	DECLARE @NXS_ExternalDataSource varchar(200)
	DECLARE @result nvarchar(200)

	SELECT @fieldType = DATA_TYPE, @schemaName = TABLE_SCHEMA 
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE 
		 TABLE_NAME = @sourceTableName AND 
		 COLUMN_NAME = @sourceFieldName
		 
	SELECT @NXS_ExternalDataSource = CAST(value AS nvarchar(max)) 
	FROM fn_listextendedproperty (NULL, 'schema', @schemaName, 'table', @sourceTableName, 'column', @sourceFieldName)
	WHERE name = 'NXS_ExternalDataSource'

	IF @NXS_ExternalDataSource IS NULL
	BEGIN
	--IF	CAST(@fieldType as varchar(128)) = 'int' -- only interested in 'int' lookups for the moment - can remove 'if' in future
	--BEGIN
		-- find the name of the lookup table
		SET @fkTable = dbo.Audit_GetForeignKeyTable( @sourceTableName, @sourceFieldName )
		
		IF @fkTable IS NOT NULL
		BEGIN
			RETURN [dbo].[Audit_GetUserFriendlyValue]( @ID, @fkTable )
		END
	END
	ELSE
	BEGIN
		--SELECT @NXS_ExternalDataSource = Source FROM [dbo].[AuditExternalDataConfig] 
		--WHERE TableName = @sourceTableName AND FieldName = @sourceFieldName
					
		IF LOWER(@NXS_ExternalDataSource) = 'cis'
		BEGIN
			SELECT @result = Title FROM common.XDB_CIS_View_Post WHERE ID = @ID	
		END
			
		ELSE IF LOWER(@NXS_ExternalDataSource) = 'cisgroup'
		BEGIN

			SELECT @result = Name FROM common.XDB_CIS_View_Group WHERE ID = @ID
		END
			
		ELSE IF LOWER(@NXS_ExternalDataSource) = 'status'
		BEGIN

            SELECT @result =
            CASE 
                WHEN @ID = '0' THEN 'Disabled'
                WHEN @ID = '1' THEN 'Enabled'
                WHEN @ID = '2' THEN 'Removed'
            END 
		END
		ELSE IF LOWER(@NXS_ExternalDataSource) = 'fitted'
		BEGIN

            SELECT @result =
            CASE 
                WHEN @ID = '0' THEN 'Not set'
                WHEN @ID = '1' THEN 'On'
                WHEN @ID = '2' THEN 'Off'
            END 
		END
        ELSE IF LOWER(@NXS_ExternalDataSource) = 'geometrytype'
		BEGIN

            SELECT @result =
            CASE                  
                WHEN @ID = '1' THEN 'Point'
                WHEN @ID = '2' THEN 'Circle'
                WHEN @ID = '3' THEN 'Rectangle'
                WHEN @ID = '4' THEN 'Polygon'                    
            END 
		END
        ELSE IF LOWER(@NXS_ExternalDataSource) = 'dms'
        BEGIN 
            
            -- Link to Synonym for DMS metadata table to get document name            
            SELECT @result = nvarchar1 
            FROM common.vwDMS_Document_MetaData 
            WHERE DocumentID = @ID
                
        END    
		ELSE IF LOWER(@NXS_ExternalDataSource) = 'parentsection'
		BEGIN
			IF @ID IS NULL
				SET @result = '<top level>'
			ELSE
				SELECT @result = ISNULL(RTRIM(ISNULL(Reference, '') + ' ' + ISNULL(Title, '')), 'UNKNOWN')
				FROM deb.Section
				WHERE Id = @ID
		END

		RETURN COALESCE(@result, 'UNKNOWN')
				
	END
	-- default to returning NULL
	RETURN NULL 

END
");

			migrationBuilder.Sql(@"
CREATE OR ALTER   TRIGGER [deb].[Section_ChangeTracking]
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

					END

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER FUNCTION [dbo].[Audit_GetLookupValue]
(
	@sourceTableName varchar( 128 ),
	@sourceFieldName varchar( 128 ),
	@ID varchar( 128 )
)
RETURNS nvarchar( 128 )
AS
BEGIN
	DECLARE @fkTable varchar(100)
	DECLARE @fieldType varchar(100)
	DECLARE @schemaName varchar(100)
	DECLARE @NXS_ExternalDataSource varchar(200)
	DECLARE @result nvarchar(200)

	SELECT @fieldType = DATA_TYPE, @schemaName = TABLE_SCHEMA 
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE 
		 TABLE_NAME = @sourceTableName AND 
		 COLUMN_NAME = @sourceFieldName
		 
	SELECT @NXS_ExternalDataSource = CAST(value AS nvarchar(max)) 
	FROM fn_listextendedproperty (NULL, 'schema', @schemaName, 'table', @sourceTableName, 'column', @sourceFieldName)
	WHERE name = 'NXS_ExternalDataSource'

	IF @NXS_ExternalDataSource IS NULL
	BEGIN
	--IF	CAST(@fieldType as varchar(128)) = 'int' -- only interested in 'int' lookups for the moment - can remove 'if' in future
	--BEGIN
		-- find the name of the lookup table
		SET @fkTable = dbo.Audit_GetForeignKeyTable( @sourceTableName, @sourceFieldName )
		
		IF @fkTable IS NOT NULL
		BEGIN
			RETURN [dbo].[Audit_GetUserFriendlyValue]( @ID, @fkTable )
		END
	END
	ELSE
	BEGIN
		--SELECT @NXS_ExternalDataSource = Source FROM [dbo].[AuditExternalDataConfig] 
		--WHERE TableName = @sourceTableName AND FieldName = @sourceFieldName
					
		IF LOWER(@NXS_ExternalDataSource) = 'cis'
		BEGIN
			SELECT @result = Title FROM common.XDB_CIS_View_Post WHERE ID = @ID	
		END
			
		ELSE IF LOWER(@NXS_ExternalDataSource) = 'cisgroup'
		BEGIN

			SELECT @result = Name FROM common.XDB_CIS_View_Group WHERE ID = @ID
		END
			
		ELSE IF LOWER(@NXS_ExternalDataSource) = 'status'
		BEGIN

            SELECT @result =
            CASE 
                WHEN @ID = '0' THEN 'Disabled'
                WHEN @ID = '1' THEN 'Enabled'
                WHEN @ID = '2' THEN 'Removed'
            END 
		END
		ELSE IF LOWER(@NXS_ExternalDataSource) = 'fitted'
		BEGIN

            SELECT @result =
            CASE 
                WHEN @ID = '0' THEN 'Not set'
                WHEN @ID = '1' THEN 'On'
                WHEN @ID = '2' THEN 'Off'
            END 
		END
        ELSE IF LOWER(@NXS_ExternalDataSource) = 'geometrytype'
		BEGIN

            SELECT @result =
            CASE                  
                WHEN @ID = '1' THEN 'Point'
                WHEN @ID = '2' THEN 'Circle'
                WHEN @ID = '3' THEN 'Rectangle'
                WHEN @ID = '4' THEN 'Polygon'                    
            END 
		END
        ELSE IF LOWER(@NXS_ExternalDataSource) = 'dms'
        BEGIN 
            
            -- Link to Synonym for DMS metadata table to get document name            
            SELECT @result = nvarchar1 
            FROM common.vwDMS_Document_MetaData 
            WHERE DocumentID = @ID
                
        END    

		RETURN COALESCE(@result, 'UNKNOWN')
				
	END
	-- default to returning NULL
	RETURN NULL 

END
			");

            migrationBuilder.Sql(@"
CREATE OR ALTER   TRIGGER [deb].[Section_ChangeTracking]
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
        }
    }
}
