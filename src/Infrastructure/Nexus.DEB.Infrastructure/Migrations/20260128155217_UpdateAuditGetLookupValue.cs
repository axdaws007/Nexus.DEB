using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuditGetLookupValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Update Audit_GetLookupValue to use correct CIS views
			migrationBuilder.Sql(@"
/****** Object:  UserDefinedFunction [dbo].[Audit_GetLookupValue]    Script Date: 28/01/2026 14:51:17 ******/
DROP FUNCTION IF EXISTS [dbo].[Audit_GetLookupValue]
GO

/****** Object:  UserDefinedFunction [dbo].[Audit_GetLookupValue]    Script Date: 28/01/2026 14:51:17 ******/
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

GO
");

			migrationBuilder.Sql(@"
EXEC sys.sp_addextendedproperty 
	@name = N'NXS_ExternalDataSource',
	@value = N'cis',
	@level0type = N'SCHEMA', @level0name = N'common',
	@level1type = N'TABLE',  @level1name = N'EntityHead',
	@level2type = N'COLUMN', @level2name = N'OwnedById';

EXEC sys.sp_addextendedproperty 
	@name = N'NXS_ExternalDataSource',
	@value = N'cisgroup',
	@level0type = N'SCHEMA', @level0name = N'common',
	@level1type = N'TABLE',  @level1name = N'EntityHead',
	@level2type = N'COLUMN', @level2name = N'OwnedByGroupId';");
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"
				EXEC sys.sp_dropextendedproperty 
					@name = N'NXS_ExternalDataSource',
					@level0type = N'SCHEMA', @level0name = N'common',
					@level1type = N'TABLE',  @level1name = N'EntityHead',
					@level2type = N'COLUMN', @level2name = N'OwnedById';

				EXEC sys.sp_dropextendedproperty 
					@name = N'NXS_ExternalDataSource',
					@level0type = N'SCHEMA', @level0name = N'common',
					@level1type = N'TABLE',  @level1name = N'EntityHead',
					@level2type = N'COLUMN', @level2name = N'OwnedByGroupId';");

			// Update Audit_GetLookupValue to use correct CIS views
			migrationBuilder.Sql(@"
/****** Object:  UserDefinedFunction [dbo].[Audit_GetLookupValue]    Script Date: 28/01/2026 14:51:17 ******/
DROP FUNCTION IF EXISTS [dbo].[Audit_GetLookupValue]
GO

/****** Object:  UserDefinedFunction [dbo].[Audit_GetLookupValue]    Script Date: 28/01/2026 14:51:17 ******/
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
		}
    }
}
