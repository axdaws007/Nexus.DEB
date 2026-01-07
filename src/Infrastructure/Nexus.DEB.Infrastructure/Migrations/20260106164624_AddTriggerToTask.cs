using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTriggerToTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"ALTER TABLE [deb].[Task] DISABLE TRIGGER [Task_ChangeTracking]
GO
DROP TRIGGER IF EXISTS [deb].[Task_ChangeTracking]
GO");
		}
    }
}
