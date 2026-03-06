using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedChangeHistoryTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update EntityHead Trigger
            migrationBuilder.Sql(@"
/****** Object:  Trigger [EntityHead_ChangeTracking]    Script Date: 06/03/2026 14:38:48 ******/
DROP TRIGGER [common].[EntityHead_ChangeTracking]
GO

/****** Object:  Trigger [common].[EntityHead_ChangeTracking]    Script Date: 06/03/2026 14:38:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE trigger [common].[EntityHead_ChangeTracking] on [common].[EntityHead] for  update, delete
as

DECLARE @bit INT ,
		@field INT ,
		@maxfield INT ,
		@char INT ,
		@fieldname VARCHAR(128) ,
		@TableName VARCHAR(128) ,
		@PKCols VARCHAR(128) ,
		@sql NVARCHAR(2000), 
		@UpdateDate VARCHAR(21) ,
		@UserName NVARCHAR(128) ,
		@Type CHAR(1) ,
		@PKFieldSelect VARCHAR(128),
		@PKValueSelect VARCHAR(128),
		@ChangeEventId UNIQUEIDENTIFIER,
		@ChangeRecordId INT,
		@Comments NVARCHAR(MAX),
		@isFieldExcluded bit,
		@fieldType nvarchar(100),
		@friendlyValue nvarchar(max)

	-- Generic way of getting table name
    SELECT @TableName = (OBJECT_NAME( parent_id )) 
    FROM sys.triggers 
    WHERE object_id = @@PROCID

	-- date and user
    SELECT @UserName = dbo.Audit_GetUserContext()

	-- Action
    IF EXISTS (SELECT * FROM Inserted)
        IF EXISTS (SELECT * FROM Deleted)
		BEGIN
            SELECT @Type = 'U'
			SELECT @Comments = 'Entity updated'
		END
        ELSE
		BEGIN
            SELECT @Type = 'I'
			SELECT @Comments = 'New entity created'
		END
    ELSE 
	BEGIN
		SELECT @Type = 'D'
		SELECT @Comments = 'Entity removed'
	END

	-- get list of columns
    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED

	-- Get primary key columns for full outer join
    SELECT @PKCols = coalesce(@PKCols + ' and', ' on') + ' i.' + c.COLUMN_NAME + ' = d.' + c.COLUMN_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
    WHERE pk.TABLE_NAME = @TableName
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'
        AND c.TABLE_NAME = pk.TABLE_NAME
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

	-- Get primary key fields select for insert
    SELECT @PKFieldSelect = coalesce(@PKFieldSelect+'+','') + '''' + COLUMN_NAME + '''' 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
    WHERE pk.TABLE_NAME = @TableName
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'
        AND c.TABLE_NAME = pk.TABLE_NAME
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

    SELECT @PKValueSelect = coalesce(@PKValueSelect+'+','') + 'convert(varchar(100), coalesce(i.' + COLUMN_NAME + ',d.' + COLUMN_NAME + '))'
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,    
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c   
    WHERE  pk.TABLE_NAME = @TableName   
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'   
        AND c.TABLE_NAME = pk.TABLE_NAME   
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME 

    IF @PKCols IS NULL
    BEGIN
        RAISERROR('no PK on table %s', 16, -1, @TableName)
        RETURN
    END


	----------------------------------------------------------------------------------
	---			Check to see that we have changes requiring auditing			   ---
	----------------------------------------------------------------------------------
	BEGIN
		DECLARE @AffectedFields TABLE(columnIndex int, columnName nvarchar(100), columnType nvarchar(100))

		SELECT @field = 0, @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) 
		FROM INFORMATION_SCHEMA.COLUMNS 
		WHERE TABLE_NAME = @TableName

		WHILE @field < @maxfield
		BEGIN
			-- FW: Must use column id for the check against COLUMNS_UPDATE() as that uses column ids NOT ordinal positions
			;WITH field AS(
				SELECT MIN(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) AS ColumnID
				FROM INFORMATION_SCHEMA.COLUMNS 
				WHERE TABLE_NAME = @TableName AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID') > @field		
			) SELECT @field = f.ColumnID,		
				@fieldname = COLUMN_NAME,
				@fieldType = DATA_TYPE
			FROM field f INNER JOIN
				INFORMATION_SCHEMA.COLUMNS sc
					ON f.ColumnID = COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')
			WHERE TABLE_NAME = @TableName        

			-- If the column has been modified, or if the operation is an Insert or Delete
			IF (SUBSTRING(COLUMNS_UPDATED(),(@field - 1) / 8 + 1, 1)) & POWER(2, (@field - 1) % 8) = POWER(2, (@field - 1) % 8)
				OR @Type in ('I','D')
			BEGIN
    			    
				SELECT @isFieldExcluded = [dbo].[Audit_IsFieldExcluded](@TableName, @fieldname) -- Make sure the field isn't excluded from Change History
				IF @isFieldExcluded = 0
				BEGIN
					INSERT INTO @AffectedFields(columnIndex, columnName, columnType)
					SELECT @field, @fieldName, @fieldType
				END
			END
		END
	END

	IF EXISTS(Select 1 From @AffectedFields)
	BEGIN
		----------------------------------------------------------------------------------
		---						Add/Get parent ChangeRecord							   ---
		----------------------------------------------------------------------------------
		BEGIN
			SELECT @ChangeEventId = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'EventId'))
			IF @ChangeEventId IS NULL
			BEGIN
				-- fallback (e.g. manual SQL scripts, ad hoc changes)
				SET @ChangeEventId = NEWID()
			END

			IF EXISTS(Select 1 From common.ChangeRecord Where EventId = @ChangeEventId)
			BEGIN
				SELECT @ChangeRecordId = Id FROM common.ChangeRecord WHERE EventId = @ChangeEventId
			END
			ELSE
			BEGIN
				EXEC dbo.CreateChangeRecordWithinTrigger  @PKValueSelect = @PKValueSelect, 
																			@PkCols = @PkCols, 
																			@Comments = @Comments,
																			@UserName = @Username,
																			@ChangeEventId = @ChangeEventId,
																			@ChangeRecordId = @ChangeRecordId OUTPUT
			END
		END

		----------------------------------------------------------------------------------
		---  For each column in the table, create a ChangeRecordItem where neccesary   ---
		----------------------------------------------------------------------------------
		SELECT @field=0
		WHILE EXISTS(Select 1 From @AffectedFields Where columnIndex > @field)
		BEGIN
			SELECT TOP 1
				@field = columnIndex,
				@fieldName = columnName,
				@fieldType = columnType
			FROM @AffectedFields
			WHERE columnIndex > @field
			ORDER BY columnIndex ASC

			-- insert a record into ChangeRecordItem for the column
			SELECT @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
			SELECT @sql = @sql + ' SELECT ''' + CAST(@ChangeRecordId AS NVARCHAR(30)) + ''''
			SELECT @sql = @sql + ',''' + @fieldname + ''''
			SELECT @sql = @sql + ',''' + dbo.Audit_GetFieldDescription(@TableName, @fieldname) + ''''
    			
			IF CAST(@fieldType as varchar(128)) = 'bit'
			BEGIN
				SELECT @sql = @sql + ', CASE WHEN d.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
				SELECT @sql = @sql + ', CASE WHEN i.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
			END
			ELSE
			BEGIN
				SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) IS NOT NULL 
				THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) 
				ELSE convert(varchar(max),d.' + @fieldname + ') END'
				SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) IS NOT NULL 
				THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) 
				ELSE convert(varchar(max),i.' + @fieldname + ') END'
			END
    			
			SELECT @sql = @sql + ', 0'
			SELECT @sql = @sql + ' FROM #ins i FULL OUTER JOIN #del d'
			SELECT @sql = @sql + @PKCols
			SELECT @sql = @sql + ' WHERE i.' + @fieldname + ' <> d.' + @fieldname 
			SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NULL AND  d.' + @fieldname + ' IS NOT NULL)' 
			SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NOT NULL AND  d.' + @fieldname + ' IS NULL)' 
			--PRINT @SQL
			EXEC (@sql)
		END
	END
	ELSE
	BEGIN
		PRINT 'No changes require auditing for [common].[EntityHead]'
	END
GO

ALTER TABLE [common].[EntityHead] ENABLE TRIGGER [EntityHead_ChangeTracking]
GO
");

            // Update Scope Trigger
			migrationBuilder.Sql(@"
/****** Object:  Trigger [Scope_ChangeTracking]    Script Date: 06/03/2026 14:39:18 ******/
DROP TRIGGER [deb].[Scope_ChangeTracking]
GO

/****** Object:  Trigger [deb].[Scope_ChangeTracking]    Script Date: 06/03/2026 14:39:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE trigger [deb].[Scope_ChangeTracking] on [deb].[Scope] for  update, delete
as

DECLARE @bit INT ,
		@field INT ,
		@maxfield INT ,
		@char INT ,
		@fieldname VARCHAR(128) ,
		@TableName VARCHAR(128) ,
		@PKCols VARCHAR(128) ,
		@sql NVARCHAR(2000), 
		@UpdateDate VARCHAR(21) ,
		@UserName NVARCHAR(128) ,
		@Type CHAR(1) ,
		@PKFieldSelect VARCHAR(128),
		@PKValueSelect VARCHAR(128),
		@ChangeEventId UNIQUEIDENTIFIER,
		@ChangeRecordId INT,
		@Comments NVARCHAR(MAX),
		@isFieldExcluded bit,
		@fieldType nvarchar(100),
		@friendlyValue nvarchar(max)

	-- Generic way of getting table name
    SELECT @TableName = (OBJECT_NAME( parent_id )) 
    FROM sys.triggers 
    WHERE object_id = @@PROCID

	-- date and user
    SELECT @UserName = dbo.Audit_GetUserContext()

	-- Action
    IF EXISTS (SELECT * FROM Inserted)
        IF EXISTS (SELECT * FROM Deleted)
		BEGIN
            SELECT @Type = 'U'
			SELECT @Comments = 'Entity updated'
		END
        ELSE
		BEGIN
            SELECT @Type = 'I'
			SELECT @Comments = 'New entity created'
		END
    ELSE 
	BEGIN
		SELECT @Type = 'D'
		SELECT @Comments = 'Entity removed'
	END

	-- get list of columns
    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED

	BEGIN
		-- Get primary key columns for full outer join
		SELECT @PKCols = coalesce(@PKCols + ' and', ' on') + ' i.' + c.COLUMN_NAME + ' = d.' + c.COLUMN_NAME
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
			INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
		WHERE pk.TABLE_NAME = @TableName
			AND CONSTRAINT_TYPE = 'PRIMARY KEY'
			AND c.TABLE_NAME = pk.TABLE_NAME
			AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

		-- Get primary key fields select for insert
		SELECT @PKFieldSelect = coalesce(@PKFieldSelect+'+','') + '''' + COLUMN_NAME + '''' 
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
			INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
		WHERE pk.TABLE_NAME = @TableName
			AND CONSTRAINT_TYPE = 'PRIMARY KEY'
			AND c.TABLE_NAME = pk.TABLE_NAME
			AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

		SELECT @PKValueSelect = coalesce(@PKValueSelect+'+','') + 'convert(varchar(100), coalesce(i.' + COLUMN_NAME + ',d.' + COLUMN_NAME + '))'
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,    
			INFORMATION_SCHEMA.KEY_COLUMN_USAGE c   
		WHERE  pk.TABLE_NAME = @TableName   
			AND CONSTRAINT_TYPE = 'PRIMARY KEY'   
			AND c.TABLE_NAME = pk.TABLE_NAME   
			AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME 

		IF @PKCols IS NULL
		BEGIN
			RAISERROR('no PK on table %s', 16, -1, @TableName)
			RETURN
		END
	END

	----------------------------------------------------------------------------------
	---			Check to see that we have changes requiring auditing			   ---
	----------------------------------------------------------------------------------
	BEGIN
		DECLARE @AffectedFields TABLE(columnIndex int, columnName nvarchar(100), columnType nvarchar(100))

		SELECT @field = 0, @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) 
		FROM INFORMATION_SCHEMA.COLUMNS 
		WHERE TABLE_NAME = @TableName

		WHILE @field < @maxfield
		BEGIN
			-- FW: Must use column id for the check against COLUMNS_UPDATE() as that uses column ids NOT ordinal positions
			;WITH field AS(
				SELECT MIN(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) AS ColumnID
				FROM INFORMATION_SCHEMA.COLUMNS 
				WHERE TABLE_NAME = @TableName AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID') > @field		
			) SELECT @field = f.ColumnID,		
				@fieldname = COLUMN_NAME,
				@fieldType = DATA_TYPE
			FROM field f INNER JOIN
				INFORMATION_SCHEMA.COLUMNS sc
					ON f.ColumnID = COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')
			WHERE TABLE_NAME = @TableName        

			-- If the column has been modified, or if the operation is an Insert or Delete
			IF (SUBSTRING(COLUMNS_UPDATED(),(@field - 1) / 8 + 1, 1)) & POWER(2, (@field - 1) % 8) = POWER(2, (@field - 1) % 8)
				OR @Type in ('I','D')
			BEGIN
    			    
				SELECT @isFieldExcluded = [dbo].[Audit_IsFieldExcluded](@TableName, @fieldname) -- Make sure the field isn't excluded from Change History
				IF @isFieldExcluded = 0
				BEGIN
					INSERT INTO @AffectedFields(columnIndex, columnName, columnType)
					SELECT @field, @fieldName, @fieldType
				END
			END
		END
	END

	IF EXISTS(Select 1 From @AffectedFields)
	BEGIN
		----------------------------------------------------------------------------------
		---						Add/Get parent ChangeRecord							   ---
		----------------------------------------------------------------------------------
		BEGIN
			SELECT @ChangeEventId = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'EventId'))
			IF @ChangeEventId IS NULL
			BEGIN
				-- fallback (e.g. manual SQL scripts, ad hoc changes)
				SET @ChangeEventId = NEWID()
			END

			IF EXISTS(Select 1 From common.ChangeRecord Where EventId = @ChangeEventId)
			BEGIN
				SELECT @ChangeRecordId = Id FROM common.ChangeRecord WHERE EventId = @ChangeEventId
			END
			ELSE
			BEGIN
				EXEC dbo.CreateChangeRecordWithinTrigger  @PKValueSelect = @PKValueSelect, 
																			@PkCols = @PkCols, 
																			@Comments = @Comments,
																			@UserName = @Username,
																			@ChangeEventId = @ChangeEventId,
																			@ChangeRecordId = @ChangeRecordId OUTPUT
			END
		END

		----------------------------------------------------------------------------------
		---  For each column in the table, create a ChangeRecordItem where neccesary   ---
		----------------------------------------------------------------------------------
		SELECT @field=0
		WHILE EXISTS(Select 1 From @AffectedFields Where columnIndex > @field)
		BEGIN
			SELECT TOP 1
				@field = columnIndex,
				@fieldName = columnName,
				@fieldType = columnType
			FROM @AffectedFields
			WHERE columnIndex > @field
			ORDER BY columnIndex ASC

			-- insert a record into ChangeRecordItem for the column
			SELECT @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
			SELECT @sql = @sql + ' SELECT ''' + CAST(@ChangeRecordId AS NVARCHAR(30)) + ''''
			SELECT @sql = @sql + ',''' + @fieldname + ''''
			SELECT @sql = @sql + ',''' + dbo.Audit_GetFieldDescription(@TableName, @fieldname) + ''''
    			
			IF CAST(@fieldType as varchar(128)) = 'bit'
			BEGIN
				SELECT @sql = @sql + ', CASE WHEN d.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
				SELECT @sql = @sql + ', CASE WHEN i.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
			END
			ELSE
			BEGIN
				SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) IS NOT NULL 
				THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) 
				ELSE convert(varchar(max),d.' + @fieldname + ') END'
				SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) IS NOT NULL 
				THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) 
				ELSE convert(varchar(max),i.' + @fieldname + ') END'
			END
    			
			SELECT @sql = @sql + ', 0'
			SELECT @sql = @sql + ' FROM #ins i FULL OUTER JOIN #del d'
			SELECT @sql = @sql + @PKCols
			SELECT @sql = @sql + ' WHERE i.' + @fieldname + ' <> d.' + @fieldname 
			SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NULL AND  d.' + @fieldname + ' IS NOT NULL)' 
			SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NOT NULL AND  d.' + @fieldname + ' IS NULL)' 
			--PRINT @SQL
			EXEC (@sql)
		END
	END
	ELSE
	BEGIN
		PRINT 'No changes require auditing for [deb].[Scope]'
	END
GO

ALTER TABLE [deb].[Scope] ENABLE TRIGGER [Scope_ChangeTracking]
GO
");

            // Update StandardVersion Trigger
			migrationBuilder.Sql(@"
/****** Object:  Trigger [StandardVersion_ChangeTracking]    Script Date: 06/03/2026 14:39:45 ******/
DROP TRIGGER [deb].[StandardVersion_ChangeTracking]
GO

/****** Object:  Trigger [deb].[StandardVersion_ChangeTracking]    Script Date: 06/03/2026 14:39:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE trigger [deb].[StandardVersion_ChangeTracking] on [deb].[StandardVersion] for  update, delete
as

DECLARE @bit INT ,
		@field INT ,
		@maxfield INT ,
		@char INT ,
		@fieldname VARCHAR(128) ,
		@TableName VARCHAR(128) ,
		@PKCols VARCHAR(128) ,
		@sql NVARCHAR(2000), 
		@UpdateDate VARCHAR(21) ,
		@UserName NVARCHAR(128) ,
		@Type CHAR(1) ,
		@PKFieldSelect VARCHAR(128),
		@PKValueSelect VARCHAR(128),
		@ChangeEventId UNIQUEIDENTIFIER,
		@ChangeRecordId INT,
		@Comments NVARCHAR(MAX),
		@isFieldExcluded bit,
		@fieldType nvarchar(100),
		@friendlyValue nvarchar(max)

	-- Generic way of getting table name
    SELECT @TableName = (OBJECT_NAME( parent_id )) 
    FROM sys.triggers 
    WHERE object_id = @@PROCID

	-- date and user
    SELECT @UserName = dbo.Audit_GetUserContext()

	-- Action
    IF EXISTS (SELECT * FROM Inserted)
        IF EXISTS (SELECT * FROM Deleted)
		BEGIN
            SELECT @Type = 'U'
			SELECT @Comments = 'Entity updated'
		END
        ELSE
		BEGIN
            SELECT @Type = 'I'
			SELECT @Comments = 'New entity created'
		END
    ELSE 
	BEGIN
		SELECT @Type = 'D'
		SELECT @Comments = 'Entity removed'
	END

	-- get list of columns
    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED

	BEGIN
		-- Get primary key columns for full outer join
		SELECT @PKCols = coalesce(@PKCols + ' and', ' on') + ' i.' + c.COLUMN_NAME + ' = d.' + c.COLUMN_NAME
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
			INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
		WHERE pk.TABLE_NAME = @TableName
			AND CONSTRAINT_TYPE = 'PRIMARY KEY'
			AND c.TABLE_NAME = pk.TABLE_NAME
			AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

		-- Get primary key fields select for insert
		SELECT @PKFieldSelect = coalesce(@PKFieldSelect+'+','') + '''' + COLUMN_NAME + '''' 
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
			INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
		WHERE pk.TABLE_NAME = @TableName
			AND CONSTRAINT_TYPE = 'PRIMARY KEY'
			AND c.TABLE_NAME = pk.TABLE_NAME
			AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

		SELECT @PKValueSelect = coalesce(@PKValueSelect+'+','') + 'convert(varchar(100), coalesce(i.' + COLUMN_NAME + ',d.' + COLUMN_NAME + '))'
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,    
			INFORMATION_SCHEMA.KEY_COLUMN_USAGE c   
		WHERE  pk.TABLE_NAME = @TableName   
			AND CONSTRAINT_TYPE = 'PRIMARY KEY'   
			AND c.TABLE_NAME = pk.TABLE_NAME   
			AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME 

		IF @PKCols IS NULL
		BEGIN
			RAISERROR('no PK on table %s', 16, -1, @TableName)
			RETURN
		END
	END

	----------------------------------------------------------------------------------
	---			Check to see that we have changes requiring auditing			   ---
	----------------------------------------------------------------------------------
	BEGIN
		DECLARE @AffectedFields TABLE(columnIndex int, columnName nvarchar(100), columnType nvarchar(100))

		SELECT @field = 0, @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) 
		FROM INFORMATION_SCHEMA.COLUMNS 
		WHERE TABLE_NAME = @TableName

		WHILE @field < @maxfield
		BEGIN
			-- FW: Must use column id for the check against COLUMNS_UPDATE() as that uses column ids NOT ordinal positions
			;WITH field AS(
				SELECT MIN(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) AS ColumnID
				FROM INFORMATION_SCHEMA.COLUMNS 
				WHERE TABLE_NAME = @TableName AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID') > @field		
			) SELECT @field = f.ColumnID,		
				@fieldname = COLUMN_NAME,
				@fieldType = DATA_TYPE
			FROM field f INNER JOIN
				INFORMATION_SCHEMA.COLUMNS sc
					ON f.ColumnID = COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')
			WHERE TABLE_NAME = @TableName        

			-- If the column has been modified, or if the operation is an Insert or Delete
			IF (SUBSTRING(COLUMNS_UPDATED(),(@field - 1) / 8 + 1, 1)) & POWER(2, (@field - 1) % 8) = POWER(2, (@field - 1) % 8)
				OR @Type in ('I','D')
			BEGIN
    			    
				SELECT @isFieldExcluded = [dbo].[Audit_IsFieldExcluded](@TableName, @fieldname) -- Make sure the field isn't excluded from Change History
				IF @isFieldExcluded = 0
				BEGIN
					INSERT INTO @AffectedFields(columnIndex, columnName, columnType)
					SELECT @field, @fieldName, @fieldType
				END
			END
		END
	END

	IF EXISTS(Select 1 From @AffectedFields)
	BEGIN
		----------------------------------------------------------------------------------
		---						Add/Get parent ChangeRecord							   ---
		----------------------------------------------------------------------------------
		BEGIN
			SELECT @ChangeEventId = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'EventId'))
			IF @ChangeEventId IS NULL
			BEGIN
				-- fallback (e.g. manual SQL scripts, ad hoc changes)
				SET @ChangeEventId = NEWID()
			END

			IF EXISTS(Select 1 From common.ChangeRecord Where EventId = @ChangeEventId)
			BEGIN
				SELECT @ChangeRecordId = Id FROM common.ChangeRecord WHERE EventId = @ChangeEventId
			END
			ELSE
			BEGIN
				EXEC dbo.CreateChangeRecordWithinTrigger  @PKValueSelect = @PKValueSelect, 
																			@PkCols = @PkCols, 
																			@Comments = @Comments,
																			@UserName = @Username,
																			@ChangeEventId = @ChangeEventId,
																			@ChangeRecordId = @ChangeRecordId OUTPUT
			END
		END

		----------------------------------------------------------------------------------
		---  For each column in the table, create a ChangeRecordItem where neccesary   ---
		----------------------------------------------------------------------------------
		SELECT @field=0
		WHILE EXISTS(Select 1 From @AffectedFields Where columnIndex > @field)
		BEGIN
			SELECT TOP 1
				@field = columnIndex,
				@fieldName = columnName,
				@fieldType = columnType
			FROM @AffectedFields
			WHERE columnIndex > @field
			ORDER BY columnIndex ASC

			-- insert a record into ChangeRecordItem for the column
			SELECT @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
			SELECT @sql = @sql + ' SELECT ''' + CAST(@ChangeRecordId AS NVARCHAR(30)) + ''''
			SELECT @sql = @sql + ',''' + @fieldname + ''''
			SELECT @sql = @sql + ',''' + dbo.Audit_GetFieldDescription(@TableName, @fieldname) + ''''
    			
			IF CAST(@fieldType as varchar(128)) = 'bit'
			BEGIN
				SELECT @sql = @sql + ', CASE WHEN d.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
				SELECT @sql = @sql + ', CASE WHEN i.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
			END
			ELSE
			BEGIN
				SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) IS NOT NULL 
				THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) 
				ELSE convert(varchar(max),d.' + @fieldname + ') END'
				SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) IS NOT NULL 
				THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) 
				ELSE convert(varchar(max),i.' + @fieldname + ') END'
			END
    			
			SELECT @sql = @sql + ', 0'
			SELECT @sql = @sql + ' FROM #ins i FULL OUTER JOIN #del d'
			SELECT @sql = @sql + @PKCols
			SELECT @sql = @sql + ' WHERE i.' + @fieldname + ' <> d.' + @fieldname 
			SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NULL AND  d.' + @fieldname + ' IS NOT NULL)' 
			SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NOT NULL AND  d.' + @fieldname + ' IS NULL)' 
			--PRINT @SQL
			EXEC (@sql)
		END
	END
	ELSE
	BEGIN
		PRINT 'No changes require auditing for [deb].[StandardVersion]'
	END
GO

ALTER TABLE [deb].[StandardVersion] ENABLE TRIGGER [StandardVersion_ChangeTracking]
GO
");

            // Update Statement Trigger
			migrationBuilder.Sql(@"
/****** Object:  Trigger [Statement_ChangeTracking]    Script Date: 06/03/2026 14:40:23 ******/
DROP TRIGGER [deb].[Statement_ChangeTracking]
GO

/****** Object:  Trigger [deb].[Statement_ChangeTracking]    Script Date: 06/03/2026 14:40:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE trigger [deb].[Statement_ChangeTracking] on [deb].[Statement] for  update, delete
as

DECLARE @bit INT ,
		@field INT ,
		@maxfield INT ,
		@char INT ,
		@fieldname VARCHAR(128) ,
		@TableName VARCHAR(128) ,
		@PKCols VARCHAR(128) ,
		@sql NVARCHAR(2000), 
		@UpdateDate VARCHAR(21) ,
		@UserName NVARCHAR(128) ,
		@Type CHAR(1) ,
		@PKFieldSelect VARCHAR(128),
		@PKValueSelect VARCHAR(128),
		@ChangeEventId UNIQUEIDENTIFIER,
		@ChangeRecordId INT,
		@Comments NVARCHAR(MAX),
		@isFieldExcluded bit,
		@fieldType nvarchar(100),
		@friendlyValue nvarchar(max)

	-- Generic way of getting table name
    SELECT @TableName = (OBJECT_NAME( parent_id )) 
    FROM sys.triggers 
    WHERE object_id = @@PROCID

	-- date and user
    SELECT @UserName = dbo.Audit_GetUserContext()

	-- Action
    IF EXISTS (SELECT * FROM Inserted)
        IF EXISTS (SELECT * FROM Deleted)
		BEGIN
            SELECT @Type = 'U'
			SELECT @Comments = 'Entity updated'
		END
        ELSE
		BEGIN
            SELECT @Type = 'I'
			SELECT @Comments = 'New entity created'
		END
    ELSE 
	BEGIN
		SELECT @Type = 'D'
		SELECT @Comments = 'Entity removed'
	END

	-- get list of columns
    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED

	BEGIN
		-- Get primary key columns for full outer join
		SELECT @PKCols = coalesce(@PKCols + ' and', ' on') + ' i.' + c.COLUMN_NAME + ' = d.' + c.COLUMN_NAME
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
			INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
		WHERE pk.TABLE_NAME = @TableName
			AND CONSTRAINT_TYPE = 'PRIMARY KEY'
			AND c.TABLE_NAME = pk.TABLE_NAME
			AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

		-- Get primary key fields select for insert
		SELECT @PKFieldSelect = coalesce(@PKFieldSelect+'+','') + '''' + COLUMN_NAME + '''' 
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
			INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
		WHERE pk.TABLE_NAME = @TableName
			AND CONSTRAINT_TYPE = 'PRIMARY KEY'
			AND c.TABLE_NAME = pk.TABLE_NAME
			AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

		SELECT @PKValueSelect = coalesce(@PKValueSelect+'+','') + 'convert(varchar(100), coalesce(i.' + COLUMN_NAME + ',d.' + COLUMN_NAME + '))'
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,    
			INFORMATION_SCHEMA.KEY_COLUMN_USAGE c   
		WHERE  pk.TABLE_NAME = @TableName   
			AND CONSTRAINT_TYPE = 'PRIMARY KEY'   
			AND c.TABLE_NAME = pk.TABLE_NAME   
			AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME 

		IF @PKCols IS NULL
		BEGIN
			RAISERROR('no PK on table %s', 16, -1, @TableName)
			RETURN
		END
	END

	----------------------------------------------------------------------------------
	---			Check to see that we have changes requiring auditing			   ---
	----------------------------------------------------------------------------------
	BEGIN
		DECLARE @AffectedFields TABLE(columnIndex int, columnName nvarchar(100), columnType nvarchar(100))

		SELECT @field = 0, @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) 
		FROM INFORMATION_SCHEMA.COLUMNS 
		WHERE TABLE_NAME = @TableName

		WHILE @field < @maxfield
		BEGIN
			-- FW: Must use column id for the check against COLUMNS_UPDATE() as that uses column ids NOT ordinal positions
			;WITH field AS(
				SELECT MIN(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) AS ColumnID
				FROM INFORMATION_SCHEMA.COLUMNS 
				WHERE TABLE_NAME = @TableName AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID') > @field		
			) SELECT @field = f.ColumnID,		
				@fieldname = COLUMN_NAME,
				@fieldType = DATA_TYPE
			FROM field f INNER JOIN
				INFORMATION_SCHEMA.COLUMNS sc
					ON f.ColumnID = COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')
			WHERE TABLE_NAME = @TableName        

			-- If the column has been modified, or if the operation is an Insert or Delete
			IF (SUBSTRING(COLUMNS_UPDATED(),(@field - 1) / 8 + 1, 1)) & POWER(2, (@field - 1) % 8) = POWER(2, (@field - 1) % 8)
				OR @Type in ('I','D')
			BEGIN
    			    
				SELECT @isFieldExcluded = [dbo].[Audit_IsFieldExcluded](@TableName, @fieldname) -- Make sure the field isn't excluded from Change History
				IF @isFieldExcluded = 0
				BEGIN
					INSERT INTO @AffectedFields(columnIndex, columnName, columnType)
					SELECT @field, @fieldName, @fieldType
				END
			END
		END
	END

	IF EXISTS(Select 1 From @AffectedFields)
	BEGIN
		----------------------------------------------------------------------------------
		---						Add/Get parent ChangeRecord							   ---
		----------------------------------------------------------------------------------
		BEGIN
			SELECT @ChangeEventId = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'EventId'))
			IF @ChangeEventId IS NULL
			BEGIN
				-- fallback (e.g. manual SQL scripts, ad hoc changes)
				SET @ChangeEventId = NEWID()
			END

			IF EXISTS(Select 1 From common.ChangeRecord Where EventId = @ChangeEventId)
			BEGIN
				SELECT @ChangeRecordId = Id FROM common.ChangeRecord WHERE EventId = @ChangeEventId
			END
			ELSE
			BEGIN
				EXEC dbo.CreateChangeRecordWithinTrigger  @PKValueSelect = @PKValueSelect, 
																			@PkCols = @PkCols, 
																			@Comments = @Comments,
																			@UserName = @Username,
																			@ChangeEventId = @ChangeEventId,
																			@ChangeRecordId = @ChangeRecordId OUTPUT
			END
		END

		----------------------------------------------------------------------------------
		---  For each column in the table, create a ChangeRecordItem where neccesary   ---
		----------------------------------------------------------------------------------
		SELECT @field=0
		WHILE EXISTS(Select 1 From @AffectedFields Where columnIndex > @field)
		BEGIN
			SELECT TOP 1
				@field = columnIndex,
				@fieldName = columnName,
				@fieldType = columnType
			FROM @AffectedFields
			WHERE columnIndex > @field
			ORDER BY columnIndex ASC

			-- insert a record into ChangeRecordItem for the column
			SELECT @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
			SELECT @sql = @sql + ' SELECT ''' + CAST(@ChangeRecordId AS NVARCHAR(30)) + ''''
			SELECT @sql = @sql + ',''' + @fieldname + ''''
			SELECT @sql = @sql + ',''' + dbo.Audit_GetFieldDescription(@TableName, @fieldname) + ''''
    			
			IF CAST(@fieldType as varchar(128)) = 'bit'
			BEGIN
				SELECT @sql = @sql + ', CASE WHEN d.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
				SELECT @sql = @sql + ', CASE WHEN i.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
			END
			ELSE
			BEGIN
				SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) IS NOT NULL 
				THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) 
				ELSE convert(varchar(max),d.' + @fieldname + ') END'
				SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) IS NOT NULL 
				THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) 
				ELSE convert(varchar(max),i.' + @fieldname + ') END'
			END
    			
			SELECT @sql = @sql + ', 0'
			SELECT @sql = @sql + ' FROM #ins i FULL OUTER JOIN #del d'
			SELECT @sql = @sql + @PKCols
			SELECT @sql = @sql + ' WHERE i.' + @fieldname + ' <> d.' + @fieldname 
			SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NULL AND  d.' + @fieldname + ' IS NOT NULL)' 
			SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NOT NULL AND  d.' + @fieldname + ' IS NULL)' 
			--PRINT @SQL
			EXEC (@sql)
		END
	END
	ELSE
	BEGIN
		PRINT 'No changes require auditing for [deb].[Statement]'
	END
GO

ALTER TABLE [deb].[Statement] ENABLE TRIGGER [Statement_ChangeTracking]
GO
");

            // Update Task Trigger
			migrationBuilder.Sql(@"
/****** Object:  Trigger [Task_ChangeTracking]    Script Date: 06/03/2026 14:40:47 ******/
DROP TRIGGER [deb].[Task_ChangeTracking]
GO

/****** Object:  Trigger [deb].[Task_ChangeTracking]    Script Date: 06/03/2026 14:40:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE trigger [deb].[Task_ChangeTracking] on [deb].[Task] for  update, delete
as

DECLARE @bit INT ,
		@field INT ,
		@maxfield INT ,
		@char INT ,
		@fieldname VARCHAR(128) ,
		@TableName VARCHAR(128) ,
		@PKCols VARCHAR(128) ,
		@sql NVARCHAR(2000), 
		@UpdateDate VARCHAR(21) ,
		@UserName NVARCHAR(128) ,
		@Type CHAR(1) ,
		@PKFieldSelect VARCHAR(128),
		@PKValueSelect VARCHAR(128),
		@ChangeEventId UNIQUEIDENTIFIER,
		@ChangeRecordId INT,
		@Comments NVARCHAR(MAX),
		@isFieldExcluded bit,
		@fieldType nvarchar(100),
		@friendlyValue nvarchar(max)

	-- Generic way of getting table name
    SELECT @TableName = (OBJECT_NAME( parent_id )) 
    FROM sys.triggers 
    WHERE object_id = @@PROCID

	-- date and user
    SELECT @UserName = dbo.Audit_GetUserContext()

	-- Action
    IF EXISTS (SELECT * FROM Inserted)
        IF EXISTS (SELECT * FROM Deleted)
		BEGIN
            SELECT @Type = 'U'
			SELECT @Comments = 'Entity updated'
		END
        ELSE
		BEGIN
            SELECT @Type = 'I'
			SELECT @Comments = 'New entity created'
		END
    ELSE 
	BEGIN
		SELECT @Type = 'D'
		SELECT @Comments = 'Entity removed'
	END

	-- get list of columns
    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED

	BEGIN
		-- Get primary key columns for full outer join
		SELECT @PKCols = coalesce(@PKCols + ' and', ' on') + ' i.' + c.COLUMN_NAME + ' = d.' + c.COLUMN_NAME
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
			INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
		WHERE pk.TABLE_NAME = @TableName
			AND CONSTRAINT_TYPE = 'PRIMARY KEY'
			AND c.TABLE_NAME = pk.TABLE_NAME
			AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

		-- Get primary key fields select for insert
		SELECT @PKFieldSelect = coalesce(@PKFieldSelect+'+','') + '''' + COLUMN_NAME + '''' 
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
			INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
		WHERE pk.TABLE_NAME = @TableName
			AND CONSTRAINT_TYPE = 'PRIMARY KEY'
			AND c.TABLE_NAME = pk.TABLE_NAME
			AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

		SELECT @PKValueSelect = coalesce(@PKValueSelect+'+','') + 'convert(varchar(100), coalesce(i.' + COLUMN_NAME + ',d.' + COLUMN_NAME + '))'
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,    
			INFORMATION_SCHEMA.KEY_COLUMN_USAGE c   
		WHERE  pk.TABLE_NAME = @TableName   
			AND CONSTRAINT_TYPE = 'PRIMARY KEY'   
			AND c.TABLE_NAME = pk.TABLE_NAME   
			AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME 

		IF @PKCols IS NULL
		BEGIN
			RAISERROR('no PK on table %s', 16, -1, @TableName)
			RETURN
		END
	END

	----------------------------------------------------------------------------------
	---			Check to see that we have changes requiring auditing			   ---
	----------------------------------------------------------------------------------
	BEGIN
		DECLARE @AffectedFields TABLE(columnIndex int, columnName nvarchar(100), columnType nvarchar(100))

		SELECT @field = 0, @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) 
		FROM INFORMATION_SCHEMA.COLUMNS 
		WHERE TABLE_NAME = @TableName

		WHILE @field < @maxfield
		BEGIN
			-- FW: Must use column id for the check against COLUMNS_UPDATE() as that uses column ids NOT ordinal positions
			;WITH field AS(
				SELECT MIN(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) AS ColumnID
				FROM INFORMATION_SCHEMA.COLUMNS 
				WHERE TABLE_NAME = @TableName AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID') > @field		
			) SELECT @field = f.ColumnID,		
				@fieldname = COLUMN_NAME,
				@fieldType = DATA_TYPE
			FROM field f INNER JOIN
				INFORMATION_SCHEMA.COLUMNS sc
					ON f.ColumnID = COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')
			WHERE TABLE_NAME = @TableName        

			-- If the column has been modified, or if the operation is an Insert or Delete
			IF (SUBSTRING(COLUMNS_UPDATED(),(@field - 1) / 8 + 1, 1)) & POWER(2, (@field - 1) % 8) = POWER(2, (@field - 1) % 8)
				OR @Type in ('I','D')
			BEGIN
    			    
				SELECT @isFieldExcluded = [dbo].[Audit_IsFieldExcluded](@TableName, @fieldname) -- Make sure the field isn't excluded from Change History
				IF @isFieldExcluded = 0
				BEGIN
					INSERT INTO @AffectedFields(columnIndex, columnName, columnType)
					SELECT @field, @fieldName, @fieldType
				END
			END
		END
	END

	IF EXISTS(Select 1 From @AffectedFields)
	BEGIN
		----------------------------------------------------------------------------------
		---						Add/Get parent ChangeRecord							   ---
		----------------------------------------------------------------------------------
		BEGIN
			SELECT @ChangeEventId = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'EventId'))
			IF @ChangeEventId IS NULL
			BEGIN
				-- fallback (e.g. manual SQL scripts, ad hoc changes)
				SET @ChangeEventId = NEWID()
			END

			IF EXISTS(Select 1 From common.ChangeRecord Where EventId = @ChangeEventId)
			BEGIN
				SELECT @ChangeRecordId = Id FROM common.ChangeRecord WHERE EventId = @ChangeEventId
			END
			ELSE
			BEGIN
				EXEC dbo.CreateChangeRecordWithinTrigger  @PKValueSelect = @PKValueSelect, 
																			@PkCols = @PkCols, 
																			@Comments = @Comments,
																			@UserName = @Username,
																			@ChangeEventId = @ChangeEventId,
																			@ChangeRecordId = @ChangeRecordId OUTPUT
			END
		END

		----------------------------------------------------------------------------------
		---  For each column in the table, create a ChangeRecordItem where neccesary   ---
		----------------------------------------------------------------------------------
		SELECT @field=0
		WHILE EXISTS(Select 1 From @AffectedFields Where columnIndex > @field)
		BEGIN
			SELECT TOP 1
				@field = columnIndex,
				@fieldName = columnName,
				@fieldType = columnType
			FROM @AffectedFields
			WHERE columnIndex > @field
			ORDER BY columnIndex ASC

			-- insert a record into ChangeRecordItem for the column
			SELECT @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
			SELECT @sql = @sql + ' SELECT ''' + CAST(@ChangeRecordId AS NVARCHAR(30)) + ''''
			SELECT @sql = @sql + ',''' + @fieldname + ''''
			SELECT @sql = @sql + ',''' + dbo.Audit_GetFieldDescription(@TableName, @fieldname) + ''''
    			
			IF CAST(@fieldType as varchar(128)) = 'bit'
			BEGIN
				SELECT @sql = @sql + ', CASE WHEN d.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
				SELECT @sql = @sql + ', CASE WHEN i.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
			END
			ELSE
			BEGIN
				SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) IS NOT NULL 
				THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) 
				ELSE convert(varchar(max),d.' + @fieldname + ') END'
				SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) IS NOT NULL 
				THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) 
				ELSE convert(varchar(max),i.' + @fieldname + ') END'
			END
    			
			SELECT @sql = @sql + ', 0'
			SELECT @sql = @sql + ' FROM #ins i FULL OUTER JOIN #del d'
			SELECT @sql = @sql + @PKCols
			SELECT @sql = @sql + ' WHERE i.' + @fieldname + ' <> d.' + @fieldname 
			SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NULL AND  d.' + @fieldname + ' IS NOT NULL)' 
			SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NOT NULL AND  d.' + @fieldname + ' IS NULL)' 
			--PRINT @SQL
			EXEC (@sql)
		END
	END
	ELSE
	BEGIN
		PRINT 'No changes require auditing for [deb].[Task]'
	END
GO

ALTER TABLE [deb].[Task] ENABLE TRIGGER [Task_ChangeTracking]
GO
");

            // ADD Requirement Trigger
			migrationBuilder.Sql(@"
/****** Object:  Trigger [Requirement_ChangeTracking]    Script Date: 06/03/2026 14:37:57 ******/
DROP TRIGGER IF EXISTS [deb].[Requirement_ChangeTracking]
GO

/****** Object:  Trigger [deb].[Requirement_ChangeTracking]    Script Date: 06/03/2026 14:37:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






CREATE trigger [deb].[Requirement_ChangeTracking] on [deb].[Requirement] for  update, delete
as

DECLARE @bit INT ,
		@field INT ,
		@maxfield INT ,
		@char INT ,
		@fieldname VARCHAR(128) ,
		@TableName VARCHAR(128) ,
		@PKCols VARCHAR(128) ,
		@sql NVARCHAR(2000), 
		@UpdateDate VARCHAR(21) ,
		@UserName NVARCHAR(128) ,
		@Type CHAR(1) ,
		@PKFieldSelect VARCHAR(128),
		@PKValueSelect VARCHAR(128),
		@ChangeEventId UNIQUEIDENTIFIER,
		@ChangeRecordId INT,
		@Comments NVARCHAR(MAX),
		@isFieldExcluded bit,
		@fieldType nvarchar(100),
		@friendlyValue nvarchar(max)

	-- Generic way of getting table name
    SELECT @TableName = (OBJECT_NAME( parent_id )) 
    FROM sys.triggers 
    WHERE object_id = @@PROCID

	-- date and user
    SELECT @UserName = dbo.Audit_GetUserContext()

	-- Action
    IF EXISTS (SELECT * FROM Inserted)
        IF EXISTS (SELECT * FROM Deleted)
		BEGIN
            SELECT @Type = 'U'
			SELECT @Comments = 'Entity updated'
		END
        ELSE
		BEGIN
            SELECT @Type = 'I'
			SELECT @Comments = 'New entity created'
		END
    ELSE 
	BEGIN
		SELECT @Type = 'D'
		SELECT @Comments = 'Entity removed'
	END

	-- get list of columns
    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED

	BEGIN
		-- Get primary key columns for full outer join
		SELECT @PKCols = coalesce(@PKCols + ' and', ' on') + ' i.' + c.COLUMN_NAME + ' = d.' + c.COLUMN_NAME
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
			INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
		WHERE pk.TABLE_NAME = @TableName
			AND CONSTRAINT_TYPE = 'PRIMARY KEY'
			AND c.TABLE_NAME = pk.TABLE_NAME
			AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

		-- Get primary key fields select for insert
		SELECT @PKFieldSelect = coalesce(@PKFieldSelect+'+','') + '''' + COLUMN_NAME + '''' 
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
			INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
		WHERE pk.TABLE_NAME = @TableName
			AND CONSTRAINT_TYPE = 'PRIMARY KEY'
			AND c.TABLE_NAME = pk.TABLE_NAME
			AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

		SELECT @PKValueSelect = coalesce(@PKValueSelect+'+','') + 'convert(varchar(100), coalesce(i.' + COLUMN_NAME + ',d.' + COLUMN_NAME + '))'
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,    
			INFORMATION_SCHEMA.KEY_COLUMN_USAGE c   
		WHERE  pk.TABLE_NAME = @TableName   
			AND CONSTRAINT_TYPE = 'PRIMARY KEY'   
			AND c.TABLE_NAME = pk.TABLE_NAME   
			AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME 

		IF @PKCols IS NULL
		BEGIN
			RAISERROR('no PK on table %s', 16, -1, @TableName)
			RETURN
		END
	END

	----------------------------------------------------------------------------------
	---			Check to see that we have changes requiring auditing			   ---
	----------------------------------------------------------------------------------
	BEGIN
		DECLARE @AffectedFields TABLE(columnIndex int, columnName nvarchar(100), columnType nvarchar(100))

		SELECT @field = 0, @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) 
		FROM INFORMATION_SCHEMA.COLUMNS 
		WHERE TABLE_NAME = @TableName

		WHILE @field < @maxfield
		BEGIN
			-- FW: Must use column id for the check against COLUMNS_UPDATE() as that uses column ids NOT ordinal positions
			;WITH field AS(
				SELECT MIN(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) AS ColumnID
				FROM INFORMATION_SCHEMA.COLUMNS 
				WHERE TABLE_NAME = @TableName AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID') > @field		
			) SELECT @field = f.ColumnID,		
				@fieldname = COLUMN_NAME,
				@fieldType = DATA_TYPE
			FROM field f INNER JOIN
				INFORMATION_SCHEMA.COLUMNS sc
					ON f.ColumnID = COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')
			WHERE TABLE_NAME = @TableName        

			-- If the column has been modified, or if the operation is an Insert or Delete
			IF (SUBSTRING(COLUMNS_UPDATED(),(@field - 1) / 8 + 1, 1)) & POWER(2, (@field - 1) % 8) = POWER(2, (@field - 1) % 8)
				OR @Type in ('I','D')
			BEGIN
    			    
				SELECT @isFieldExcluded = [dbo].[Audit_IsFieldExcluded](@TableName, @fieldname) -- Make sure the field isn't excluded from Change History
				IF @isFieldExcluded = 0
				BEGIN
					INSERT INTO @AffectedFields(columnIndex, columnName, columnType)
					SELECT @field, @fieldName, @fieldType
				END
			END
		END
	END

	IF EXISTS(Select 1 From @AffectedFields)
	BEGIN
		----------------------------------------------------------------------------------
		---						Add/Get parent ChangeRecord							   ---
		----------------------------------------------------------------------------------
		BEGIN
			SELECT @ChangeEventId = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'EventId'))
			IF @ChangeEventId IS NULL
			BEGIN
				-- fallback (e.g. manual SQL scripts, ad hoc changes)
				SET @ChangeEventId = NEWID()
			END

			IF EXISTS(Select 1 From common.ChangeRecord Where EventId = @ChangeEventId)
			BEGIN
				SELECT @ChangeRecordId = Id FROM common.ChangeRecord WHERE EventId = @ChangeEventId
			END
			ELSE
			BEGIN
				EXEC dbo.CreateChangeRecordWithinTrigger  @PKValueSelect = @PKValueSelect, 
																			@PkCols = @PkCols, 
																			@Comments = @Comments,
																			@UserName = @Username,
																			@ChangeEventId = @ChangeEventId,
																			@ChangeRecordId = @ChangeRecordId OUTPUT
			END
		END

		----------------------------------------------------------------------------------
		---  For each column in the table, create a ChangeRecordItem where neccesary   ---
		----------------------------------------------------------------------------------
		SELECT @field=0
		WHILE EXISTS(Select 1 From @AffectedFields Where columnIndex > @field)
		BEGIN
			SELECT TOP 1
				@field = columnIndex,
				@fieldName = columnName,
				@fieldType = columnType
			FROM @AffectedFields
			WHERE columnIndex > @field
			ORDER BY columnIndex ASC

			-- insert a record into ChangeRecordItem for the column
			SELECT @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
			SELECT @sql = @sql + ' SELECT ''' + CAST(@ChangeRecordId AS NVARCHAR(30)) + ''''
			SELECT @sql = @sql + ',''' + @fieldname + ''''
			SELECT @sql = @sql + ',''' + dbo.Audit_GetFieldDescription(@TableName, @fieldname) + ''''
    			
			IF CAST(@fieldType as varchar(128)) = 'bit'
			BEGIN
				SELECT @sql = @sql + ', CASE WHEN d.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
				SELECT @sql = @sql + ', CASE WHEN i.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
			END
			ELSE
			BEGIN
				SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) IS NOT NULL 
				THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) 
				ELSE convert(varchar(max),d.' + @fieldname + ') END'
				SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) IS NOT NULL 
				THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) 
				ELSE convert(varchar(max),i.' + @fieldname + ') END'
			END
    			
			SELECT @sql = @sql + ', 0'
			SELECT @sql = @sql + ' FROM #ins i FULL OUTER JOIN #del d'
			SELECT @sql = @sql + @PKCols
			SELECT @sql = @sql + ' WHERE i.' + @fieldname + ' <> d.' + @fieldname 
			SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NULL AND  d.' + @fieldname + ' IS NOT NULL)' 
			SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NOT NULL AND  d.' + @fieldname + ' IS NULL)' 
			--PRINT @SQL
			EXEC (@sql)
		END
	END
	ELSE
	BEGIN
		PRINT 'No changes require auditing for [deb].[Requirement]'
	END
GO

ALTER TABLE [deb].[Requirement] ENABLE TRIGGER [Requirement_ChangeTracking]
GO
");
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			// DROP Requirement Trigger
			migrationBuilder.Sql(@"
DROP TRIGGER IF EXISTS [deb].[Requirement_ChangeTracking]
GO
");

			// Revert Task Trigger
			migrationBuilder.Sql(@"
/****** Object:  Trigger [Task_ChangeTracking]    Script Date: 06/01/2026 16:44:57 ******/
DROP TRIGGER IF EXISTS [deb].[Task_ChangeTracking]
GO

/****** Object:  Trigger [deb].[Task_ChangeTracking]    Script Date: 06/01/2026 16:44:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE trigger [deb].[Task_ChangeTracking] on [deb].[Task] for  update, delete
as

DECLARE @bit INT ,
    @field INT ,
    @maxfield INT ,
    @char INT ,
    @fieldname VARCHAR(128) ,
    @TableName VARCHAR(128) ,
    @PKCols VARCHAR(128) ,
    @sql NVARCHAR(2000), 
    @UpdateDate VARCHAR(21) ,
    @UserName NVARCHAR(128) ,
    @Type CHAR(1) ,
    @PKFieldSelect VARCHAR(128),
    @PKValueSelect VARCHAR(128),
	@ChangeEventId UNIQUEIDENTIFIER,
	@ChangeRecordId INT,
	@Comments NVARCHAR(MAX)

	-- Generic way of getting table name
    SELECT @TableName = (OBJECT_NAME( parent_id )) 
    FROM sys.triggers 
    WHERE object_id = @@PROCID

	-- date and user
    SELECT @UserName = dbo.Audit_GetUserContext()

	-- Action
    IF EXISTS (SELECT * FROM Inserted)
        IF EXISTS (SELECT * FROM Deleted)
		BEGIN
            SELECT @Type = 'U'
			SELECT @Comments = 'Entity updated'
		END
        ELSE
		BEGIN
            SELECT @Type = 'I'
			SELECT @Comments = 'New entity created'
		END
    ELSE 
	BEGIN
		SELECT @Type = 'D'
		SELECT @Comments = 'Entity removed'
	END

	-- get list of columns
    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED

-- Get primary key columns for full outer join
    SELECT @PKCols = coalesce(@PKCols + ' and', ' on') + ' i.' + c.COLUMN_NAME + ' = d.' + c.COLUMN_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
    WHERE pk.TABLE_NAME = @TableName
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'
        AND c.TABLE_NAME = pk.TABLE_NAME
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

-- Get primary key fields select for insert
    SELECT @PKFieldSelect = coalesce(@PKFieldSelect+'+','') + '''' + COLUMN_NAME + '''' 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
    WHERE pk.TABLE_NAME = @TableName
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'
        AND c.TABLE_NAME = pk.TABLE_NAME
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

    SELECT @PKValueSelect = coalesce(@PKValueSelect+'+','') + 'convert(varchar(100), coalesce(i.' + COLUMN_NAME + ',d.' + COLUMN_NAME + '))'
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,    
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c   
    WHERE  pk.TABLE_NAME = @TableName   
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'   
        AND c.TABLE_NAME = pk.TABLE_NAME   
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME 

    IF @PKCols IS NULL
    BEGIN
		PRINT 'PKCols IS NULL'
        RAISERROR('no PK on table %s', 16, -1, @TableName)
        RETURN
    END

	SELECT @ChangeEventId = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'EventId'))
	IF @ChangeEventId IS NULL
	BEGIN
		-- fallback (e.g. manual SQL scripts, ad hoc changes)
		SET @ChangeEventId = NEWID()
	END

	IF EXISTS(Select 1 From common.ChangeRecord Where EventId = @ChangeEventId)
	BEGIN
		SELECT @ChangeRecordId = Id FROM common.ChangeRecord WHERE EventId = @ChangeEventId
	END
	ELSE
	BEGIN
		EXEC dbo.CreateChangeRecordWithinTrigger @PKValueSelect = @PKValueSelect, 
												 @PkCols = @PkCols, 
												 @Comments = @Comments,
												 @UserName = @Username,
												 @ChangeEventId = @ChangeEventId,
												 @ChangeRecordId = @ChangeRecordId OUTPUT
	END

	-- Loop through each column in the table
    SELECT @field = 0, @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = @TableName

    DECLARE @isFieldExcluded bit
    DECLARE @fieldType nvarchar(100)
    DECLARE @friendlyValue nvarchar(max)

    WHILE @field < @maxfield
    BEGIN

        -- FW: Must use column id for the check against COLUMNS_UPDATE() as that uses column ids NOT ordinal positions
	    ;WITH field AS(
            SELECT MIN(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) AS ColumnID
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = @TableName AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID') > @field		
        ) SELECT @field = f.ColumnID,		
            @fieldname = COLUMN_NAME,
            @fieldType = DATA_TYPE
        FROM field f INNER JOIN
            INFORMATION_SCHEMA.COLUMNS sc
                ON f.ColumnID = COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')
        WHERE TABLE_NAME = @TableName        

		-- If the column has been modified, or if the operation is an Insert or Delete
	    IF (SUBSTRING(COLUMNS_UPDATED(),(@field - 1) / 8 + 1, 1)) & POWER(2, (@field - 1) % 8) = POWER(2, (@field - 1) % 8)
		    OR @Type in ('I','D')
	    begin
    			    
		    SELECT @isFieldExcluded = [dbo].[Audit_IsFieldExcluded](@TableName, @fieldname) -- Make sure the field isn't excluded from Change History
		    IF @isFieldExcluded = 0
		    BEGIN
				-- insert a record into ChangeRecordItem for the column
			    SELECT @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
			    SELECT @sql = @sql + ' SELECT ' + CAST(@ChangeRecordId AS NVARCHAR(30))
			    SELECT @sql = @sql + ',''' + @fieldname + ''''
			    SELECT @sql = @sql + ',''' + dbo.Audit_GetFieldDescription(@TableName, @fieldname) + ''''
    			
			    IF CAST(@fieldType as varchar(128)) = 'bit'
			    BEGIN
				    SELECT @sql = @sql + ', CASE WHEN d.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
				    SELECT @sql = @sql + ', CASE WHEN i.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
			    END
			    ELSE
			    BEGIN
				    SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) IS NOT NULL 
					THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) 
					ELSE convert(varchar(max),d.' + @fieldname + ') END'
				    SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) IS NOT NULL 
					THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) 
					ELSE convert(varchar(max),i.' + @fieldname + ') END'
			    END
    			
			    SELECT @sql = @sql + ', 0'
			    SELECT @sql = @sql + ' FROM #ins i FULL OUTER JOIN #del d'
			    SELECT @sql = @sql + @PKCols
			    SELECT @sql = @sql + ' WHERE i.' + @fieldname + ' <> d.' + @fieldname 
			    SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NULL AND  d.' + @fieldname + ' IS NOT NULL)' 
			    SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NOT NULL AND  d.' + @fieldname + ' IS NULL)' 
			    --PRINT @SQL
			    EXEC (@sql)
		    END
	    END
    END
GO

ALTER TABLE [deb].[Task] ENABLE TRIGGER [Task_ChangeTracking]
GO
");

			// Revert Statement Trigger
			migrationBuilder.Sql(@"
				
/****** Object:  Trigger [Statement_ChangeTracking]    Script Date: 03/12/2025 16:28:52 ******/
DROP TRIGGER IF EXISTS [deb].[Statement_ChangeTracking]
GO

/****** Object:  Trigger [deb].[Statement_ChangeTracking]    Script Date: 03/12/2025 16:28:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE trigger [deb].[Statement_ChangeTracking] on [deb].[Statement] for  update, delete
as

DECLARE @bit INT ,
    @field INT ,
    @maxfield INT ,
    @char INT ,
    @fieldname VARCHAR(128) ,
    @TableName VARCHAR(128) ,
    @PKCols VARCHAR(128) ,
    @sql NVARCHAR(2000), 
    @UpdateDate VARCHAR(21) ,
    @UserName NVARCHAR(128) ,
    @Type CHAR(1) ,
    @PKFieldSelect VARCHAR(128),
    @PKValueSelect VARCHAR(128),
	@ChangeEventId UNIQUEIDENTIFIER,
	@ChangeRecordId INT,
	@Comments NVARCHAR(MAX)

	-- Generic way of getting table name
    SELECT @TableName = (OBJECT_NAME( parent_id )) 
    FROM sys.triggers 
    WHERE object_id = @@PROCID

	-- date and user
    SELECT @UserName = dbo.Audit_GetUserContext()

	-- Action
    IF EXISTS (SELECT * FROM Inserted)
        IF EXISTS (SELECT * FROM Deleted)
		BEGIN
            SELECT @Type = 'U'
			SELECT @Comments = 'Entity updated'
		END
        ELSE
		BEGIN
            SELECT @Type = 'I'
			SELECT @Comments = 'New entity created'
		END
    ELSE 
	BEGIN
		SELECT @Type = 'D'
		SELECT @Comments = 'Entity removed'
	END

	-- get list of columns
    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED

-- Get primary key columns for full outer join
    SELECT @PKCols = coalesce(@PKCols + ' and', ' on') + ' i.' + c.COLUMN_NAME + ' = d.' + c.COLUMN_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
    WHERE pk.TABLE_NAME = @TableName
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'
        AND c.TABLE_NAME = pk.TABLE_NAME
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

-- Get primary key fields select for insert
    SELECT @PKFieldSelect = coalesce(@PKFieldSelect+'+','') + '''' + COLUMN_NAME + '''' 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
    WHERE pk.TABLE_NAME = @TableName
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'
        AND c.TABLE_NAME = pk.TABLE_NAME
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

    SELECT @PKValueSelect = coalesce(@PKValueSelect+'+','') + 'convert(varchar(100), coalesce(i.' + COLUMN_NAME + ',d.' + COLUMN_NAME + '))'
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,    
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c   
    WHERE  pk.TABLE_NAME = @TableName   
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'   
        AND c.TABLE_NAME = pk.TABLE_NAME   
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME 

    IF @PKCols IS NULL
    BEGIN
		PRINT 'PKCols IS NULL'
        RAISERROR('no PK on table %s', 16, -1, @TableName)
        RETURN
    END

	SELECT @ChangeEventId = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'EventId'))
	IF @ChangeEventId IS NULL
	BEGIN
		-- fallback (e.g. manual SQL scripts, ad hoc changes)
		SET @ChangeEventId = NEWID()
	END

	IF EXISTS(Select 1 From common.ChangeRecord Where EventId = @ChangeEventId)
	BEGIN
		SELECT @ChangeRecordId = Id FROM common.ChangeRecord WHERE EventId = @ChangeEventId
	END
	ELSE
	BEGIN
		EXEC dbo.CreateChangeRecordWithinTrigger @PKValueSelect = @PKValueSelect, 
												 @PkCols = @PkCols, 
												 @Comments = @Comments,
												 @UserName = @Username,
												 @ChangeEventId = @ChangeEventId,
												 @ChangeRecordId = @ChangeRecordId OUTPUT
	END

	-- Loop through each column in the table
    SELECT @field = 0, @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = @TableName

    DECLARE @isFieldExcluded bit
    DECLARE @fieldType nvarchar(100)
    DECLARE @friendlyValue nvarchar(max)

    WHILE @field < @maxfield
    BEGIN

        -- FW: Must use column id for the check against COLUMNS_UPDATE() as that uses column ids NOT ordinal positions
	    ;WITH field AS(
            SELECT MIN(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) AS ColumnID
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = @TableName AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID') > @field		
        ) SELECT @field = f.ColumnID,		
            @fieldname = COLUMN_NAME,
            @fieldType = DATA_TYPE
        FROM field f INNER JOIN
            INFORMATION_SCHEMA.COLUMNS sc
                ON f.ColumnID = COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')
        WHERE TABLE_NAME = @TableName        

		-- If the column has been modified, or if the operation is an Insert or Delete
	    IF (SUBSTRING(COLUMNS_UPDATED(),(@field - 1) / 8 + 1, 1)) & POWER(2, (@field - 1) % 8) = POWER(2, (@field - 1) % 8)
		    OR @Type in ('I','D')
	    begin
    			    
		    SELECT @isFieldExcluded = [dbo].[Audit_IsFieldExcluded](@TableName, @fieldname) -- Make sure the field isn't excluded from Change History
		    IF @isFieldExcluded = 0
		    BEGIN
				-- insert a record into ChangeRecordItem for the column
			    SELECT @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
			    SELECT @sql = @sql + ' SELECT ' + CAST(@ChangeRecordId AS NVARCHAR(30))
			    SELECT @sql = @sql + ',''' + @fieldname + ''''
			    SELECT @sql = @sql + ',''' + dbo.Audit_GetFieldDescription(@TableName, @fieldname) + ''''
    			
			    IF CAST(@fieldType as varchar(128)) = 'bit'
			    BEGIN
				    SELECT @sql = @sql + ', CASE WHEN d.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
				    SELECT @sql = @sql + ', CASE WHEN i.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
			    END
			    ELSE
			    BEGIN
				    SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) IS NOT NULL 
					THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) 
					ELSE convert(varchar(max),d.' + @fieldname + ') END'
				    SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) IS NOT NULL 
					THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) 
					ELSE convert(varchar(max),i.' + @fieldname + ') END'
			    END
    			
			    SELECT @sql = @sql + ', 0'
			    SELECT @sql = @sql + ' FROM #ins i FULL OUTER JOIN #del d'
			    SELECT @sql = @sql + @PKCols
			    SELECT @sql = @sql + ' WHERE i.' + @fieldname + ' <> d.' + @fieldname 
			    SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NULL AND  d.' + @fieldname + ' IS NOT NULL)' 
			    SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NOT NULL AND  d.' + @fieldname + ' IS NULL)' 
			    --PRINT @SQL
			    EXEC (@sql)
		    END
	    END
    END
GO

ALTER TABLE [deb].[Statement] ENABLE TRIGGER [Statement_ChangeTracking]
GO

            ");

			// Revert StandardVersion Trigger
			migrationBuilder.Sql(@"
-- =============================================
-- Author:		Alex Dawson
-- Create date: 25/02/26
-- Description:	Change Tracking for the StandardVersion table
-- =============================================
CREATE OR ALTER   trigger [deb].[StandardVersion_ChangeTracking] on [deb].[StandardVersion] for  update, delete
as

DECLARE @bit INT ,
    @field INT ,
    @maxfield INT ,
    @char INT ,
    @fieldname VARCHAR(128) ,
    @TableName VARCHAR(128) ,
    @PKCols VARCHAR(128) ,
    @sql NVARCHAR(2000), 
    @UpdateDate VARCHAR(21) ,
    @UserName NVARCHAR(128) ,
    @Type CHAR(1) ,
    @PKFieldSelect VARCHAR(128),
    @PKValueSelect VARCHAR(128),
	@ChangeEventId UNIQUEIDENTIFIER,
	@ChangeRecordId INT,
	@Comments NVARCHAR(MAX)

	-- Generic way of getting table name
    SELECT @TableName = (OBJECT_NAME( parent_id )) 
    FROM sys.triggers 
    WHERE object_id = @@PROCID

	-- date and user
    SELECT @UserName = dbo.Audit_GetUserContext()

	-- Action
    IF EXISTS (SELECT * FROM Inserted)
        IF EXISTS (SELECT * FROM Deleted)
		BEGIN
            SELECT @Type = 'U'
			SELECT @Comments = 'Entity updated'
		END
        ELSE
		BEGIN
            SELECT @Type = 'I'
			SELECT @Comments = 'New entity created'
		END
    ELSE 
	BEGIN
		SELECT @Type = 'D'
		SELECT @Comments = 'Entity removed'
	END

	-- get list of columns
    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED

-- Get primary key columns for full outer join
    SELECT @PKCols = coalesce(@PKCols + ' and', ' on') + ' i.' + c.COLUMN_NAME + ' = d.' + c.COLUMN_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
    WHERE pk.TABLE_NAME = @TableName
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'
        AND c.TABLE_NAME = pk.TABLE_NAME
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

-- Get primary key fields select for insert
    SELECT @PKFieldSelect = coalesce(@PKFieldSelect+'+','') + '''' + COLUMN_NAME + '''' 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
    WHERE pk.TABLE_NAME = @TableName
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'
        AND c.TABLE_NAME = pk.TABLE_NAME
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

    SELECT @PKValueSelect = coalesce(@PKValueSelect+'+','') + 'convert(varchar(100), coalesce(i.' + COLUMN_NAME + ',d.' + COLUMN_NAME + '))'
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,    
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c   
    WHERE  pk.TABLE_NAME = @TableName   
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'   
        AND c.TABLE_NAME = pk.TABLE_NAME   
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME 

    IF @PKCols IS NULL
    BEGIN
		PRINT 'PKCols IS NULL'
        RAISERROR('no PK on table %s', 16, -1, @TableName)
        RETURN
    END

	SELECT @ChangeEventId = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'EventId'))
	IF @ChangeEventId IS NULL
	BEGIN
		-- fallback (e.g. manual SQL scripts, ad hoc changes)
		SET @ChangeEventId = NEWID()
	END

	IF EXISTS(Select 1 From common.ChangeRecord Where EventId = @ChangeEventId)
	BEGIN
		SELECT @ChangeRecordId = Id FROM common.ChangeRecord WHERE EventId = @ChangeEventId
	END
	ELSE
	BEGIN
		EXEC dbo.CreateChangeRecordWithinTrigger @PKValueSelect = @PKValueSelect, 
												 @PkCols = @PkCols, 
												 @Comments = @Comments,
												 @UserName = @Username,
												 @ChangeEventId = @ChangeEventId,
												 @ChangeRecordId = @ChangeRecordId OUTPUT
	END

	-- Loop through each column in the table
    SELECT @field = 0, @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = @TableName

    DECLARE @isFieldExcluded bit
    DECLARE @fieldType nvarchar(100)
    DECLARE @friendlyValue nvarchar(max)

    WHILE @field < @maxfield
    BEGIN

        -- FW: Must use column id for the check against COLUMNS_UPDATE() as that uses column ids NOT ordinal positions
	    ;WITH field AS(
            SELECT MIN(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) AS ColumnID
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = @TableName AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID') > @field		
        ) SELECT @field = f.ColumnID,		
            @fieldname = COLUMN_NAME,
            @fieldType = DATA_TYPE
        FROM field f INNER JOIN
            INFORMATION_SCHEMA.COLUMNS sc
                ON f.ColumnID = COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')
        WHERE TABLE_NAME = @TableName        

		-- If the column has been modified, or if the operation is an Insert or Delete
	    IF (SUBSTRING(COLUMNS_UPDATED(),(@field - 1) / 8 + 1, 1)) & POWER(2, (@field - 1) % 8) = POWER(2, (@field - 1) % 8)
		    OR @Type in ('I','D')
	    begin
    			    
		    SELECT @isFieldExcluded = [dbo].[Audit_IsFieldExcluded](@TableName, @fieldname) -- Make sure the field isn't excluded from Change History
		    IF @isFieldExcluded = 0
		    BEGIN
				-- insert a record into ChangeRecordItem for the column
			    SELECT @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
			    SELECT @sql = @sql + ' SELECT ' + CAST(@ChangeRecordId AS NVARCHAR(30))
			    SELECT @sql = @sql + ',''' + @fieldname + ''''
			    SELECT @sql = @sql + ',''' + dbo.Audit_GetFieldDescription(@TableName, @fieldname) + ''''
    			
			    IF CAST(@fieldType as varchar(128)) = 'bit'
			    BEGIN
				    SELECT @sql = @sql + ', CASE WHEN d.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
				    SELECT @sql = @sql + ', CASE WHEN i.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
			    END
			    ELSE
			    BEGIN
				    SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) IS NOT NULL 
					THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) 
					ELSE convert(varchar(max),d.' + @fieldname + ') END'
				    SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) IS NOT NULL 
					THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) 
					ELSE convert(varchar(max),i.' + @fieldname + ') END'
			    END
    			
			    SELECT @sql = @sql + ', 0'
			    SELECT @sql = @sql + ' FROM #ins i FULL OUTER JOIN #del d'
			    SELECT @sql = @sql + @PKCols
			    SELECT @sql = @sql + ' WHERE i.' + @fieldname + ' <> d.' + @fieldname 
			    SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NULL AND  d.' + @fieldname + ' IS NOT NULL)' 
			    SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NOT NULL AND  d.' + @fieldname + ' IS NULL)' 
			    --PRINT @SQL
			    EXEC (@sql)
		    END
	    END
    END
            ");

			// Revert Scope Trigger
			migrationBuilder.Sql(@"
-- =============================================
-- Author:		Alex Dawson
-- Create date: 24/02/26
-- Description:	Change Tracking for the Scope table
-- =============================================
CREATE OR ALTER trigger [deb].[Scope_ChangeTracking] on [deb].[Scope] for  update, delete
as

DECLARE @bit INT ,
    @field INT ,
    @maxfield INT ,
    @char INT ,
    @fieldname VARCHAR(128) ,
    @TableName VARCHAR(128) ,
    @PKCols VARCHAR(128) ,
    @sql NVARCHAR(2000), 
    @UpdateDate VARCHAR(21) ,
    @UserName NVARCHAR(128) ,
    @Type CHAR(1) ,
    @PKFieldSelect VARCHAR(128),
    @PKValueSelect VARCHAR(128),
	@ChangeEventId UNIQUEIDENTIFIER,
	@ChangeRecordId INT,
	@Comments NVARCHAR(MAX)

	-- Generic way of getting table name
    SELECT @TableName = (OBJECT_NAME( parent_id )) 
    FROM sys.triggers 
    WHERE object_id = @@PROCID

	-- date and user
    SELECT @UserName = dbo.Audit_GetUserContext()

	-- Action
    IF EXISTS (SELECT * FROM Inserted)
        IF EXISTS (SELECT * FROM Deleted)
		BEGIN
            SELECT @Type = 'U'
			SELECT @Comments = 'Entity updated'
		END
        ELSE
		BEGIN
            SELECT @Type = 'I'
			SELECT @Comments = 'New entity created'
		END
    ELSE 
	BEGIN
		SELECT @Type = 'D'
		SELECT @Comments = 'Entity removed'
	END

	-- get list of columns
    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED

-- Get primary key columns for full outer join
    SELECT @PKCols = coalesce(@PKCols + ' and', ' on') + ' i.' + c.COLUMN_NAME + ' = d.' + c.COLUMN_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
    WHERE pk.TABLE_NAME = @TableName
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'
        AND c.TABLE_NAME = pk.TABLE_NAME
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

-- Get primary key fields select for insert
    SELECT @PKFieldSelect = coalesce(@PKFieldSelect+'+','') + '''' + COLUMN_NAME + '''' 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
    WHERE pk.TABLE_NAME = @TableName
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'
        AND c.TABLE_NAME = pk.TABLE_NAME
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

    SELECT @PKValueSelect = coalesce(@PKValueSelect+'+','') + 'convert(varchar(100), coalesce(i.' + COLUMN_NAME + ',d.' + COLUMN_NAME + '))'
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,    
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c   
    WHERE  pk.TABLE_NAME = @TableName   
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'   
        AND c.TABLE_NAME = pk.TABLE_NAME   
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME 

    IF @PKCols IS NULL
    BEGIN
		PRINT 'PKCols IS NULL'
        RAISERROR('no PK on table %s', 16, -1, @TableName)
        RETURN
    END

	SELECT @ChangeEventId = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'EventId'))
	IF @ChangeEventId IS NULL
	BEGIN
		-- fallback (e.g. manual SQL scripts, ad hoc changes)
		SET @ChangeEventId = NEWID()
	END

	IF EXISTS(Select 1 From common.ChangeRecord Where EventId = @ChangeEventId)
	BEGIN
		SELECT @ChangeRecordId = Id FROM common.ChangeRecord WHERE EventId = @ChangeEventId
	END
	ELSE
	BEGIN
		EXEC dbo.CreateChangeRecordWithinTrigger @PKValueSelect = @PKValueSelect, 
												 @PkCols = @PkCols, 
												 @Comments = @Comments,
												 @UserName = @Username,
												 @ChangeEventId = @ChangeEventId,
												 @ChangeRecordId = @ChangeRecordId OUTPUT
	END

	-- Loop through each column in the table
    SELECT @field = 0, @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = @TableName

    DECLARE @isFieldExcluded bit
    DECLARE @fieldType nvarchar(100)
    DECLARE @friendlyValue nvarchar(max)

    WHILE @field < @maxfield
    BEGIN

        -- FW: Must use column id for the check against COLUMNS_UPDATE() as that uses column ids NOT ordinal positions
	    ;WITH field AS(
            SELECT MIN(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) AS ColumnID
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = @TableName AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID') > @field		
        ) SELECT @field = f.ColumnID,		
            @fieldname = COLUMN_NAME,
            @fieldType = DATA_TYPE
        FROM field f INNER JOIN
            INFORMATION_SCHEMA.COLUMNS sc
                ON f.ColumnID = COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')
        WHERE TABLE_NAME = @TableName        

		-- If the column has been modified, or if the operation is an Insert or Delete
	    IF (SUBSTRING(COLUMNS_UPDATED(),(@field - 1) / 8 + 1, 1)) & POWER(2, (@field - 1) % 8) = POWER(2, (@field - 1) % 8)
		    OR @Type in ('I','D')
	    begin
    			    
		    SELECT @isFieldExcluded = [dbo].[Audit_IsFieldExcluded](@TableName, @fieldname) -- Make sure the field isn't excluded from Change History
		    IF @isFieldExcluded = 0
		    BEGIN
				-- insert a record into ChangeRecordItem for the column
			    SELECT @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
			    SELECT @sql = @sql + ' SELECT ' + CAST(@ChangeRecordId AS NVARCHAR(30))
			    SELECT @sql = @sql + ',''' + @fieldname + ''''
			    SELECT @sql = @sql + ',''' + dbo.Audit_GetFieldDescription(@TableName, @fieldname) + ''''
    			
			    IF CAST(@fieldType as varchar(128)) = 'bit'
			    BEGIN
				    SELECT @sql = @sql + ', CASE WHEN d.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
				    SELECT @sql = @sql + ', CASE WHEN i.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
			    END
			    ELSE
			    BEGIN
				    SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) IS NOT NULL 
					THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) 
					ELSE convert(varchar(max),d.' + @fieldname + ') END'
				    SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) IS NOT NULL 
					THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) 
					ELSE convert(varchar(max),i.' + @fieldname + ') END'
			    END
    			
			    SELECT @sql = @sql + ', 0'
			    SELECT @sql = @sql + ' FROM #ins i FULL OUTER JOIN #del d'
			    SELECT @sql = @sql + @PKCols
			    SELECT @sql = @sql + ' WHERE i.' + @fieldname + ' <> d.' + @fieldname 
			    SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NULL AND  d.' + @fieldname + ' IS NOT NULL)' 
			    SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NOT NULL AND  d.' + @fieldname + ' IS NULL)' 
			    --PRINT @SQL
			    EXEC (@sql)
		    END
	    END
    END
            ");

			// Revert EntityHead Trigger
			migrationBuilder.Sql(@"

/****** Object:  Trigger [EntityHead_ChangeTracking]    Script Date: 03/12/2025 16:30:35 ******/
DROP TRIGGER IF EXISTS [common].[EntityHead_ChangeTracking]
GO

/****** Object:  Trigger [common].[EntityHead_ChangeTracking]    Script Date: 03/12/2025 16:30:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE trigger [common].[EntityHead_ChangeTracking] on [common].[EntityHead] for  update, delete
as

DECLARE @bit INT ,
    @field INT ,
    @maxfield INT ,
    @char INT ,
    @fieldname VARCHAR(128) ,
    @TableName VARCHAR(128) ,
    @PKCols VARCHAR(128) ,
    @sql NVARCHAR(2000), 
    @UpdateDate VARCHAR(21) ,
    @UserName NVARCHAR(128) ,
    @Type CHAR(1) ,
    @PKFieldSelect VARCHAR(128),
    @PKValueSelect VARCHAR(128),
	@ChangeEventId UNIQUEIDENTIFIER,
	@ChangeRecordId INT,
	@Comments NVARCHAR(MAX)

	-- Generic way of getting table name
    SELECT @TableName = (OBJECT_NAME( parent_id )) 
    FROM sys.triggers 
    WHERE object_id = @@PROCID

	-- date and user
    SELECT @UserName = dbo.Audit_GetUserContext()

	-- Action
    IF EXISTS (SELECT * FROM Inserted)
        IF EXISTS (SELECT * FROM Deleted)
		BEGIN
            SELECT @Type = 'U'
			SELECT @Comments = 'Entity updated'
		END
        ELSE
		BEGIN
            SELECT @Type = 'I'
			SELECT @Comments = 'New entity created'
		END
    ELSE 
	BEGIN
		SELECT @Type = 'D'
		SELECT @Comments = 'Entity removed'
	END

	-- get list of columns
    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED

-- Get primary key columns for full outer join
    SELECT @PKCols = coalesce(@PKCols + ' and', ' on') + ' i.' + c.COLUMN_NAME + ' = d.' + c.COLUMN_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
    WHERE pk.TABLE_NAME = @TableName
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'
        AND c.TABLE_NAME = pk.TABLE_NAME
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

-- Get primary key fields select for insert
    SELECT @PKFieldSelect = coalesce(@PKFieldSelect+'+','') + '''' + COLUMN_NAME + '''' 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
    WHERE pk.TABLE_NAME = @TableName
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'
        AND c.TABLE_NAME = pk.TABLE_NAME
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

    SELECT @PKValueSelect = coalesce(@PKValueSelect+'+','') + 'convert(varchar(100), coalesce(i.' + COLUMN_NAME + ',d.' + COLUMN_NAME + '))'
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,    
        INFORMATION_SCHEMA.KEY_COLUMN_USAGE c   
    WHERE  pk.TABLE_NAME = @TableName   
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'   
        AND c.TABLE_NAME = pk.TABLE_NAME   
        AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME 

    IF @PKCols IS NULL
    BEGIN
        RAISERROR('no PK on table %s', 16, -1, @TableName)
        RETURN
    END

	SELECT @ChangeEventId = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'EventId'))
	IF @ChangeEventId IS NULL
	BEGIN
		-- fallback (e.g. manual SQL scripts, ad hoc changes)
		SET @ChangeEventId = NEWID()
	END

	IF EXISTS(Select 1 From common.ChangeRecord Where EventId = @ChangeEventId)
	BEGIN
		SELECT @ChangeRecordId = Id FROM common.ChangeRecord WHERE EventId = @ChangeEventId
	END
	ELSE
	BEGIN
		EXEC dbo.CreateChangeRecordWithinTrigger  @PKValueSelect = @PKValueSelect, 
																	@PkCols = @PkCols, 
																	@Comments = @Comments,
																	@UserName = @Username,
																	@ChangeEventId = @ChangeEventId,
																	@ChangeRecordId = @ChangeRecordId OUTPUT
	END

	-- Loop through each column in the table
    SELECT @field = 0, @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = @TableName

    DECLARE @isFieldExcluded bit
    DECLARE @fieldType nvarchar(100)
    DECLARE @friendlyValue nvarchar(max)

    WHILE @field < @maxfield
    BEGIN

        -- FW: Must use column id for the check against COLUMNS_UPDATE() as that uses column ids NOT ordinal positions
	    ;WITH field AS(
            SELECT MIN(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')) AS ColumnID
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = @TableName AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID') > @field		
        ) SELECT @field = f.ColumnID,		
            @fieldname = COLUMN_NAME,
            @fieldType = DATA_TYPE
        FROM field f INNER JOIN
            INFORMATION_SCHEMA.COLUMNS sc
                ON f.ColumnID = COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'ColumnID')
        WHERE TABLE_NAME = @TableName        

		-- If the column has been modified, or if the operation is an Insert or Delete
	    IF (SUBSTRING(COLUMNS_UPDATED(),(@field - 1) / 8 + 1, 1)) & POWER(2, (@field - 1) % 8) = POWER(2, (@field - 1) % 8)
		    OR @Type in ('I','D')
	    begin
    			    
		    SELECT @isFieldExcluded = [dbo].[Audit_IsFieldExcluded](@TableName, @fieldname) -- Make sure the field isn't excluded from Change History
		    IF @isFieldExcluded = 0
		    BEGIN
				-- insert a record into ChangeRecordItem for the column
			    SELECT @sql = 'INSERT INTO common.ChangeRecordItem (ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)'
			    SELECT @sql = @sql + ' SELECT ''' + CAST(@ChangeRecordId AS NVARCHAR(30)) + ''''
			    SELECT @sql = @sql + ',''' + @fieldname + ''''
			    SELECT @sql = @sql + ',''' + dbo.Audit_GetFieldDescription(@TableName, @fieldname) + ''''
    			
			    IF CAST(@fieldType as varchar(128)) = 'bit'
			    BEGIN
				    SELECT @sql = @sql + ', CASE WHEN d.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
				    SELECT @sql = @sql + ', CASE WHEN i.' + @fieldname + ' = 1 THEN ''True'' ELSE ''False'' END'
			    END
			    ELSE
			    BEGIN
				    SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) IS NOT NULL 
					THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),d.' + @fieldname + ')) 
					ELSE convert(varchar(max),d.' + @fieldname + ') END'
				    SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) IS NOT NULL 
					THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(max),i.' + @fieldname + ')) 
					ELSE convert(varchar(max),i.' + @fieldname + ') END'
			    END
    			
			    SELECT @sql = @sql + ', 0'
			    SELECT @sql = @sql + ' FROM #ins i FULL OUTER JOIN #del d'
			    SELECT @sql = @sql + @PKCols
			    SELECT @sql = @sql + ' WHERE i.' + @fieldname + ' <> d.' + @fieldname 
			    SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NULL AND  d.' + @fieldname + ' IS NOT NULL)' 
			    SELECT @sql = @sql + ' OR (i.' + @fieldname + ' IS NOT NULL AND  d.' + @fieldname + ' IS NULL)' 
			    --PRINT @SQL
			    EXEC (@sql)
		    END
	    END
    END
GO

ALTER TABLE [common].[EntityHead] ENABLE TRIGGER [EntityHead_ChangeTracking]
GO

            ");
		}
    }
}
