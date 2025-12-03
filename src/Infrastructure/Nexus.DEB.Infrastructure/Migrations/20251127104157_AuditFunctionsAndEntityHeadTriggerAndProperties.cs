using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuditFunctionsAndEntityHeadTriggerAndProperties : Migration
    {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{

			#region dbo.Audit_GetFieldDescription
			migrationBuilder.Sql(@"
                
/****** Object:  UserDefinedFunction [dbo].[Audit_GetFieldDescription]    Script Date: 26/11/2025 13:48:49 ******/
DROP FUNCTION IF EXISTS [dbo].[Audit_GetFieldDescription]
GO

/****** Object:  UserDefinedFunction [dbo].[Audit_GetFieldDescription]    Script Date: 26/11/2025 13:48:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Sean Walsh
-- Create date: 2014-06-9
-- Description:	Returns the meta data description of a field or if not set the field name.
-- http://stackoverflow.com/questions/15161505/use-a-query-to-access-column-description-in-sql
-- =============================================
CREATE FUNCTION [dbo].[Audit_GetFieldDescription]
(
	@tableName varchar(128),
	@fieldName varchar(128)
)
RETURNS nvarchar( 128 )
AS
BEGIN

	DECLARE @result nvarchar(128)

	SET @result = NULL		
	SELECT 
        @result = CAST(sep.value AS nvarchar(128))
    FROM sys.tables st
        INNER JOIN sys.columns sc ON st.object_id = sc.object_id
        INNER JOIN sys.extended_properties sep on st.object_id = sep.major_id
                                         AND sc.column_id = sep.minor_id
                                         AND sep.name = 'MS_Description'
    WHERE st.name = @tableName AND sc.name = @fieldName

    IF @result IS NULL
	BEGIN
		SET @result = @fieldName
	END
	
	IF @result LIKE '%:%'
	BEGIN
	
	    DECLARE @ModuleID UNIQUEIDENTIFIER
	    SELECT @ModuleID = ModuleID FROM dbo.ModuleInfo 
	    WHERE ModuleName = 
    	    SUBSTRING(@result, 1, PATINDEX('%:%', @result) -1)
	    
	    IF @ModuleID IS NOT NULL
	    BEGIN
	    
	        SET @result = REVERSE(SUBSTRING(REVERSE(@result), 1, PATINDEX('%:%', REVERSE(@result)) -1))
            IF (EXISTS(SELECT 1 FROM dbo.ModuleSetting WHERE Name = @result AND ModuleID = @ModuleID))
                SELECT @result = Value FROM dbo.ModuleSetting WHERE Name = @result AND ModuleID = @ModuleID	
        
        END 
    END
	RETURN @result

END


/*

SELECT [dbo].[Audit_GetFieldDescription]( 'entityhead', 'ownedbyid' )

SELECT [dbo].[Audit_GetFieldDescription]( 'entityhead', 'xxxxxx' )

*/

GO

            ");
			#endregion

			#region dbo.Audit_GetUserContext
			migrationBuilder.Sql(@"
                
/****** Object:  UserDefinedFunction [dbo].[Audit_GetUserContext]    Script Date: 26/11/2025 13:41:44 ******/
DROP FUNCTION IF EXISTS [dbo].[Audit_GetUserContext]
GO

/****** Object:  UserDefinedFunction [dbo].[Audit_GetUserContext]    Script Date: 26/11/2025 13:41:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Sean Walsh
-- Create date: 2014-06-17
-- Description:	See https://msmvps.com/blogs/p3net/archive/2013/09/13/entity-framework-and-user-context.aspx
-- =============================================
CREATE FUNCTION [dbo].[Audit_GetUserContext] ()
RETURNS VARCHAR(128)
AS
BEGIN

    DECLARE @ContextVal VARCHAR(150)
    SET @ContextVal = CONTEXT_INFO()
    
    -- Remove status flag here if present
    IF PATINDEX('%{%}%', @ContextVal) > 0
    BEGIN
    
       SET @ContextVal = LEFT (@ContextVal, PATINDEX('%{%', @ContextVal) -1) +
                         RIGHT(@ContextVal, DATALENGTH(@ContextVal) - PATINDEX('%}%', @ContextVal))       
    
    END
    
    RETURN REPLACE(COALESCE(CONVERT(VARCHAR(128), @ContextVal), SUSER_NAME()), '''','''''')           
    
END
GO


            ");
			#endregion

			#region dbo.Audit_IsFieldExcluded
			migrationBuilder.Sql(@"
            
/****** Object:  UserDefinedFunction [dbo].[Audit_IsFieldExcluded]    Script Date: 26/11/2025 14:30:01 ******/
DROP FUNCTION IF EXISTS [dbo].[Audit_IsFieldExcluded]
GO

/****** Object:  UserDefinedFunction [dbo].[Audit_IsFieldExcluded]    Script Date: 26/11/2025 14:30:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Stewart Caig
-- Create date: 24/02/2015
-- Description:	Returns a boolean indicating if the specified field should be excluded from Audit history logging
-- =============================================
CREATE FUNCTION [dbo].[Audit_IsFieldExcluded]
(
	-- Add the parameters for the function here
	@tableName varchar(128),
	@fieldName varchar(128)
)
RETURNS bit
AS
BEGIN

	DECLARE @result nvarchar(128)
	DECLARE @IsFieldExcluded bit
	SET @IsFieldExcluded = 0
	
	SET @result = NULL
	SELECT 
        --st.name [Table],
        --sc.name [Column],
        @result = CAST(sep.value AS nvarchar(128))
    FROM sys.tables st
    INNER JOIN sys.columns sc ON st.object_id = sc.object_id
    INNER JOIN sys.extended_properties sep on st.object_id = sep.major_id
                                         AND sc.column_id = sep.minor_id
                                         AND sep.name = 'NXS_IsIgnoredForAudit'
    WHERE st.name = @tableName
		and sc.name = @fieldName

	IF @result IS NOT NULL AND LOWER(@result) = 'true' 
	BEGIN
		SET @IsFieldExcluded = 1
	END
	
	-- Return the result of the function
	RETURN @IsFieldExcluded

END
GO



            ");
			#endregion

			#region dbo.Audit_GetLookupValue
			migrationBuilder.Sql(@"
/****** Object:  UserDefinedFunction [dbo].[Audit_GetLookupValue]    Script Date: 27/11/2025 10:45:19 ******/
DROP FUNCTION IF EXISTS [dbo].[Audit_GetLookupValue]
GO

/****** Object:  UserDefinedFunction [dbo].[Audit_GetLookupValue]    Script Date: 27/11/2025 10:45:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		Sean Walsh
-- Create date: 2014-06-17
-- Description:	Determines if the field represents a FK relationship and automatically looks up the value.
--				If not FK then returns NULL.
-- =============================================
CREATE FUNCTION [dbo].[Audit_GetLookupValue]
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
				SELECT @result = Title FROM common.vwCIS_View_Post WHERE ID = @ID	
			END
			
			ELSE IF LOWER(@NXS_ExternalDataSource) = 'cisgroup'
			BEGIN

				SELECT @result = Name FROM common.vwCIS_View_Group WHERE ID = @ID
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
	
	--END
	
	-- default to returning NULL
	RETURN NULL 

END

GO

");
			#endregion

			#region dbo.Audit_GetForeignKeyTable
			migrationBuilder.Sql(@"
/****** Object:  UserDefinedFunction [dbo].[Audit_GetForeignKeyTable]    Script Date: 27/11/2025 11:56:22 ******/
DROP FUNCTION IF EXISTS [dbo].[Audit_GetForeignKeyTable]
GO

/****** Object:  UserDefinedFunction [dbo].[Audit_GetForeignKeyTable]    Script Date: 27/11/2025 11:56:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Sean Walsh
-- Create date: 2014-06-09
-- Description:	Gets the foreign key table name for a FK relationship.
-- =============================================
CREATE FUNCTION [dbo].[Audit_GetForeignKeyTable]
(	
	@sourceTableName varchar(128),
	@sourceFieldName varchar(128)
)
RETURNS varchar( 128 ) 
AS 
BEGIN
	DECLARE @result varchar( 128 )

	SELECT 				
		 @result = (OBJECT_NAME (f.referenced_object_id) 	)
	FROM sys.foreign_keys AS f
	INNER JOIN sys.foreign_key_columns AS fc ON f.OBJECT_ID = fc.constraint_object_id
	WHERE OBJECT_NAME(f.parent_object_id) = @sourceTableName
		AND COL_NAME(fc.parent_object_id, fc.parent_column_id) = @sourceFieldName

	RETURN @result

END
GO

");
			#endregion

			#region dbo.vw_Audit_Custom_Lookups
			migrationBuilder.Sql(@"
/****** Object:  View [dbo].[vw_Audit_Custom_Lookups]    Script Date: 27/11/2025 11:57:43 ******/
DROP VIEW IF EXISTS [dbo].[vw_Audit_Custom_Lookups]
GO

/****** Object:  View [dbo].[vw_Audit_Custom_Lookups]    Script Date: 27/11/2025 11:57:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_Audit_Custom_Lookups]
AS

SELECT 'RequirementCategory' AS 'TableName', CONVERT(varchar(128), ID) AS 'ID', Title AS 'Title'
FROM deb.RequirementCategory

UNION ALL

SELECT 'RequirementType' AS 'TableName', CONVERT(varchar(128), ID) AS 'ID', Title AS 'Title'
FROM deb.RequirementType

UNION ALL

SELECT 'Standard' AS 'TableName', CONVERT(varchar(128), ID) AS 'ID', Title AS 'Title'
FROM deb.Standard

UNION ALL

SELECT 'TaskType' AS 'TableName', CONVERT(varchar(128), ID) AS 'ID', Title AS 'Title'
FROM deb.TaskType

GO

");
			#endregion

			#region dbo.Audit_GetUserFriendlyValue
			migrationBuilder.Sql(@"
/****** Object:  UserDefinedFunction [dbo].[Audit_GetUserFriendlyValue]    Script Date: 27/11/2025 12:03:43 ******/
DROP FUNCTION IF EXISTS [dbo].[Audit_GetUserFriendlyValue]
GO

/****** Object:  UserDefinedFunction [dbo].[Audit_GetUserFriendlyValue]    Script Date: 27/11/2025 12:03:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sean Walsh
-- Create date:	2014-06-09
-- Description:	Gets the foreign field value
-- dbo.Audit_GetUserFriendlyValue( r.[OldValue], fk.RefTableName, fk.RefPKName, md.value )
-- =============================================
CREATE FUNCTION [dbo].[Audit_GetUserFriendlyValue]
(
	@lookupValue varchar(1000),
	@refTableName varchar(128)
)
RETURNS nvarchar(1000)
AS
BEGIN
	
	DECLARE @result nvarchar(max)

	-- this may fail if foreign key is not an int
	SELECT @result = [Title] 
	FROM dbo.vw_Audit_Custom_Lookups 
	WHERE TableName = @refTableName AND ID = @lookupValue
				
	RETURN @result

END

GO

");
			#endregion

			#region EntityHead_ChangeTracking Trigger
			migrationBuilder.Sql(@"
/****** Object:  Trigger [EntityHead_ChangeTracking]    Script Date: 26/11/2025 13:34:38 ******/
DROP TRIGGER IF EXISTS [common].[EntityHead_ChangeTracking]
GO

/****** Object:  Trigger [common].[EntityHead_ChangeTracking]    Script Date: 26/11/2025 13:34:38 ******/
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

	-- Create a ChangeRecord for the event
	SELECT @sql = 'INSERT INTO common.ChangeRecord(EntityId, ChangeDate, Comments, ChangeByUser, IsDeleted)'
	SELECT @sql = @sql + ' SELECT TOP 1 ' + @PKValueSelect
	SELECT @sql = @sql + ', GETDATE()'
	SELECT @sql = @sql + ', ''' + @Comments + ''''
	SELECT @sql = @sql + ', ''' + @Username + ''''
	SELECT @sql = @sql + ', 0'
	SELECT @sql = @sql + ' FROM #ins i FULL OUTER JOIN #del d'
	SELECT @sql = @sql + @PKCols
	SELECT @sql = @sql + ' SELECT @out_id = SCOPE_IDENTITY()'
	--PRINT @sql
	EXEC sp_executeSQL @sql, N'@out_id INT OUTPUT', @out_id = @ChangeRecordId OUTPUT

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
				    SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(1000),d.' + @fieldname + ')) IS NOT NULL 
					THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(1000),d.' + @fieldname + ')) 
					ELSE convert(varchar(1000),d.' + @fieldname + ') END'
				    SELECT @sql = @sql + ', CASE WHEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(1000),i.' + @fieldname + ')) IS NOT NULL 
					THEN [dbo].[Audit_GetLookupValue](''' + @TableName + ''', ''' + @fieldname + ''', convert(nvarchar(1000),i.' + @fieldname + ')) 
					ELSE convert(varchar(1000),i.' + @fieldname + ') END'
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
			#endregion

			#region Add MS_Description Extended Property to EntityHead columns
			migrationBuilder.Sql(@"

			EXEC sys.sp_addextendedproperty 
				@name = N'MS_Description',
				@value = N'Serial Number',
				@level0type = N'SCHEMA', @level0name = N'common',
				@level1type = N'TABLE',  @level1name = N'EntityHead',
				@level2type = N'COLUMN', @level2name = N'SerialNumber';

			EXEC sys.sp_addextendedproperty 
				@name = N'MS_Description',
				@value = N'Owned By',
				@level0type = N'SCHEMA', @level0name = N'common',
				@level1type = N'TABLE',  @level1name = N'EntityHead',
				@level2type = N'COLUMN', @level2name = N'OwnedByID';

			EXEC sys.sp_addextendedproperty 
				@name = N'MS_Description',
				@value = N'Owned By Group',
				@level0type = N'SCHEMA', @level0name = N'common',
				@level1type = N'TABLE',  @level1name = N'EntityHead',
				@level2type = N'COLUMN', @level2name = N'OwnedByGroupID';

			EXEC sys.sp_addextendedproperty 
				@name = N'MS_Description',
				@value = N'Is Archived',
				@level0type = N'SCHEMA', @level0name = N'common',
				@level1type = N'TABLE',  @level1name = N'EntityHead',
				@level2type = N'COLUMN', @level2name = N'IsArchived';

			EXEC sys.sp_addextendedproperty 
				@name = N'MS_Description',
				@value = N'Created By',
				@level0type = N'SCHEMA', @level0name = N'common',
				@level1type = N'TABLE',  @level1name = N'EntityHead',
				@level2type = N'COLUMN', @level2name = N'CreatedByID';

			EXEC sys.sp_addextendedproperty 
				@name = N'MS_Description',
				@value = N'Created Date',
				@level0type = N'SCHEMA', @level0name = N'common',
				@level1type = N'TABLE',  @level1name = N'EntityHead',
				@level2type = N'COLUMN', @level2name = N'CreatedDate';

");
			#endregion

			#region Add NXS_IsIgnoredForAudit Extended Property to EntityHead columns
			migrationBuilder.Sql(@"

			EXEC sys.sp_addextendedproperty 
				@name = N'NXS_IsIgnoredForAudit',
				@value = N'true',
				@level0type = N'SCHEMA', @level0name = N'common',
				@level1type = N'TABLE',  @level1name = N'EntityHead',
				@level2type = N'COLUMN', @level2name = N'EntityID';

			EXEC sys.sp_addextendedproperty 
				@name = N'NXS_IsIgnoredForAudit',
				@value = N'true',
				@level0type = N'SCHEMA', @level0name = N'common',
				@level1type = N'TABLE',  @level1name = N'EntityHead',
				@level2type = N'COLUMN', @level2name = N'ModuleID';

			EXEC sys.sp_addextendedproperty 
				@name = N'NXS_IsIgnoredForAudit',
				@value = N'true',
				@level0type = N'SCHEMA', @level0name = N'common',
				@level1type = N'TABLE',  @level1name = N'EntityHead',
				@level2type = N'COLUMN', @level2name = N'IsRemoved';

			EXEC sys.sp_addextendedproperty 
				@name = N'NXS_IsIgnoredForAudit',
				@value = N'true',
				@level0type = N'SCHEMA', @level0name = N'common',
				@level1type = N'TABLE',  @level1name = N'EntityHead',
				@level2type = N'COLUMN', @level2name = N'LastModifiedByID';

			EXEC sys.sp_addextendedproperty 
				@name = N'NXS_IsIgnoredForAudit',
				@value = N'true',
				@level0type = N'SCHEMA', @level0name = N'common',
				@level1type = N'TABLE',  @level1name = N'EntityHead',
				@level2type = N'COLUMN', @level2name = N'LastModifiedDate';

			EXEC sys.sp_addextendedproperty 
				@name = N'NXS_IsIgnoredForAudit',
				@value = N'true',
				@level0type = N'SCHEMA', @level0name = N'common',
				@level1type = N'TABLE',  @level1name = N'EntityHead',
				@level2type = N'COLUMN', @level2name = N'EntityTypeTitle';

");
			#endregion
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"
				EXEC sys.sp_dropextendedproperty 
					@name = N'NXS_IsIgnoredForAudit',
					@level0type = N'SCHEMA', @level0name = N'common',
					@level1type = N'TABLE',  @level1name = N'EntityHead',
					@level2type = N'COLUMN', @level2name = N'EntityID';

				EXEC sys.sp_dropextendedproperty 
					@name = N'NXS_IsIgnoredForAudit',
					@level0type = N'SCHEMA', @level0name = N'common',
					@level1type = N'TABLE',  @level1name = N'EntityHead',
					@level2type = N'COLUMN', @level2name = N'ModuleID';

				EXEC sys.sp_dropextendedproperty 
					@name = N'NXS_IsIgnoredForAudit',
					@level0type = N'SCHEMA', @level0name = N'common',
					@level1type = N'TABLE',  @level1name = N'EntityHead',
					@level2type = N'COLUMN', @level2name = N'IsRemoved';

				EXEC sys.sp_dropextendedproperty 
					@name = N'NXS_IsIgnoredForAudit',
					@level0type = N'SCHEMA', @level0name = N'common',
					@level1type = N'TABLE',  @level1name = N'EntityHead',
					@level2type = N'COLUMN', @level2name = N'LastModifiedByID';

				EXEC sys.sp_dropextendedproperty 
					@name = N'NXS_IsIgnoredForAudit',
					@level0type = N'SCHEMA', @level0name = N'common',
					@level1type = N'TABLE',  @level1name = N'EntityHead',
					@level2type = N'COLUMN', @level2name = N'LastModifiedDate';

				EXEC sys.sp_dropextendedproperty 
					@name = N'NXS_IsIgnoredForAudit',
					@level0type = N'SCHEMA', @level0name = N'common',
					@level1type = N'TABLE',  @level1name = N'EntityHead',
					@level2type = N'COLUMN', @level2name = N'EntityTypeTitle';
			");

			migrationBuilder.Sql(@"
				EXEC sys.sp_dropextendedproperty 
					@name = N'MS_Description',
					@level0type = N'SCHEMA', @level0name = N'common',
					@level1type = N'TABLE',  @level1name = N'EntityHead',
					@level2type = N'COLUMN', @level2name = N'SerialNumber';

				EXEC sys.sp_dropextendedproperty 
					@name = N'MS_Description',
					@level0type = N'SCHEMA', @level0name = N'common',
					@level1type = N'TABLE',  @level1name = N'EntityHead',
					@level2type = N'COLUMN', @level2name = N'OwnedByID';

				EXEC sys.sp_dropextendedproperty 
					@name = N'MS_Description',
					@level0type = N'SCHEMA', @level0name = N'common',
					@level1type = N'TABLE',  @level1name = N'EntityHead',
					@level2type = N'COLUMN', @level2name = N'OwnedByGroupID';

				EXEC sys.sp_dropextendedproperty 
					@name = N'MS_Description',
					@level0type = N'SCHEMA', @level0name = N'common',
					@level1type = N'TABLE',  @level1name = N'EntityHead',
					@level2type = N'COLUMN', @level2name = N'IsArchived';

				EXEC sys.sp_dropextendedproperty 
					@name = N'MS_Description',
					@level0type = N'SCHEMA', @level0name = N'common',
					@level1type = N'TABLE',  @level1name = N'EntityHead',
					@level2type = N'COLUMN', @level2name = N'CreatedByID';

				EXEC sys.sp_dropextendedproperty 
					@name = N'MS_Description',
					@level0type = N'SCHEMA', @level0name = N'common',
					@level1type = N'TABLE',  @level1name = N'EntityHead',
					@level2type = N'COLUMN', @level2name = N'CreatedDate';
			");

			migrationBuilder.Sql(@"
				ALTER TABLE [common].[EntityHead] DISABLE TRIGGER [EntityHead_ChangeTracking]
				DROP TRIGGER IF EXISTS [common].[EntityHead_ChangeTracking]
            ");

			migrationBuilder.Sql(@"
				DROP FUNCTION IF EXISTS [dbo].[Audit_GetUserFriendlyValue]
            ");

			migrationBuilder.Sql(@"
                DROP FUNCTION IF EXISTS [dbo].[vw_Audit_Custom_Lookups]
            ");

			migrationBuilder.Sql(@"
                DROP FUNCTION IF EXISTS [dbo].[Audit_GetForeignKeyTable]
            ");

			migrationBuilder.Sql(@"
                DROP FUNCTION IF EXISTS [dbo].[Audit_GetLookupValue]
            ");

			migrationBuilder.Sql(@"
                DROP FUNCTION IF EXISTS [dbo].[Audit_IsFieldExcluded]
            ");

			migrationBuilder.Sql(@"
                DROP FUNCTION IF EXISTS [dbo].[Audit_GetUserContext]
            ");

			migrationBuilder.Sql(@"
                DROP FUNCTION IF EXISTS [dbo].[Audit_GetFieldDescription]
            ");
		}
	}
}
