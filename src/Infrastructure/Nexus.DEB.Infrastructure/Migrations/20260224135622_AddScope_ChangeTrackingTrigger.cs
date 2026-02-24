using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScope_ChangeTrackingTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.Sql(@"
-- =============================================
-- Author:		Alex Dawson
-- Create date: 24/02/26
-- Description:	Change Tracking for the Many to Many ScopeRequirement table
-- =============================================
CREATE OR ALTER TRIGGER [deb].[ScopeRequirement_ChangeTracking] 
   ON  [deb].[ScopeRequirement]
   AFTER Insert, Delete
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @ChangeEventId UNIQUEIDENTIFIER,
			@sql NVARCHAR(2000), 
			@TableName VARCHAR(128),
			@FriendlyTableName VARCHAR(128),
			@Type CHAR(1),
			@PKCols VARCHAR(500),
			@PKFieldSelect VARCHAR(500),
			@PKValueSelect VARCHAR(500),
			@UserName NVARCHAR(128),
			@ChangeRecordId INT,
			@ChangeRecordItemId INT,
			@ChangedFrom VARCHAR(MAX),
			@ChangedTo VARCHAR(MAX)

	SELECT @TableName = 'ScopeRequirement'
	SELECT @FriendlyTableName = 'Scope / Requirement'

	-- date and user
    SELECT @UserName = dbo.Audit_GetUserContext()

	-- get list of columns
    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED

	IF EXISTS (SELECT * FROM Inserted)
    BEGIN
        SELECT @Type = 'I'
	END
    ELSE 
	BEGIN
		SELECT @Type = 'D'
	END

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
		EXEC dbo.CreateChangeRecordWithinTrigger  @PKValueSelect = 'ISNULL(i.ScopeId, d.ScopeId)', 
																	@PkCols = @PkCols, 
																	@Comments = 'Scopes/Requirements Changed',
																	@UserName = @Username,
																	@ChangeEventId = @ChangeEventId,
																	@ChangeRecordId = @ChangeRecordId OUTPUT
	END

	IF EXISTS(Select 1 From common.ChangeRecordItem Where ChangeRecordId = @ChangeRecordId AND FieldName = @TableName)
	BEGIN
		SELECT @ChangeRecordItemId = Id FROM common.ChangeRecordItem WHERE ChangeRecordId = @ChangeRecordId AND FieldName = @TableName
	END

	-- When setting the ChangedFrom and ChangedTo values of the ChangeRecordItem, 
	-- ChangedTo is always the current state of the table
	-- ChangedFrom takes any existing value for ChangedFrom if available
	-- otherwise it is the current state of the table + deleted record, or minus the inserted record

	IF @ChangeRecordItemId IS NULL
	BEGIN
		IF @Type = 'I'
		BEGIN
			SELECT @ChangedFrom = STRING_AGG(CONCAT('Scope: ', ehScope.Title, ', Requirement: ', ehReq.SerialNumber), '; ')
			FROM deb.ScopeRequirement sr
			JOIN common.ChangeRecord cr ON cr.EntityId = sr.ScopeId
			JOIN common.EntityHead ehReq ON ehReq.EntityId = sr.RequirementId
			JOIN common.EntityHead ehScope ON ehScope.EntityId = sr.ScopeId
			LEFT JOIN INSERTED i ON i.ScopeId = sr.ScopeId AND i.RequirementId = sr.RequirementId
			WHERE cr.Id = @ChangeRecordId
			AND i.RequirementId IS NULL AND i.ScopeId IS NULL
		END
		ELSE
		BEGIN
			;WITH AllSRs AS (
				SELECT * FROM deb.ScopeRequirement sr
				UNION ALL
				SELECT * FROM DELETED
			)
			SELECT @ChangedFrom = STRING_AGG(CONCAT('Scope: ', ehScope.Title, ', Requirement: ', ehReq.SerialNumber), '; ')
			FROM AllSRs sr
			JOIN common.ChangeRecord cr ON cr.EntityId = sr.ScopeId
			JOIN common.EntityHead ehReq ON ehReq.EntityId = sr.RequirementId
			JOIN common.EntityHead ehScope ON ehScope.EntityId = sr.ScopeId
			WHERE cr.Id = @ChangeRecordId
		END

		SELECT @ChangedTo = STRING_AGG(CONCAT('Scope: ', ehScope.Title, ', Requirement: ', ehReq.SerialNumber), '; ')
		FROM deb.ScopeRequirement sr
		JOIN common.ChangeRecord cr ON cr.EntityId = sr.ScopeId
		JOIN common.EntityHead ehReq ON ehReq.EntityId = sr.RequirementId
		JOIN common.EntityHead ehScope ON ehScope.EntityId = sr.ScopeId
		WHERE cr.Id = @ChangeRecordId

		INSERT INTO common.ChangeRecordItem(ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
		SELECT @ChangeRecordId, @TableName, @FriendlyTableName, @ChangedFrom, @ChangedTo, 0
	END
	ELSE
	BEGIN
		SELECT @ChangedTo = STRING_AGG(CONCAT('Scope: ', ehScope.Title, ', Requirement: ', ehReq.SerialNumber), '; ')
		FROM deb.ScopeRequirement sr
		JOIN common.ChangeRecord cr ON cr.EntityId = sr.ScopeId
		JOIN common.EntityHead ehReq ON ehReq.EntityId = sr.RequirementId
		JOIN common.EntityHead ehScope ON ehScope.EntityId = sr.ScopeId
		WHERE cr.Id = @ChangeRecordId

		UPDATE common.ChangeRecordItem
		SET ChangedTo = @ChangedTo
		WHERE Id = @ChangeRecordItemId
	END
END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"DROP TRIGGER [deb].[Scope_ChangeTracking]");

            migrationBuilder.Sql(@"DROP TRIGGER [deb].[ScopeRequirement_ChangeTracking]");
        }
    }
}
