using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatementRequirementScopeTriggerAndTriggerMetaData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			#region Statement_ChangeTracking Trigger
			migrationBuilder.Sql(@"
/****** Object:  Trigger [StatementRequirementScope_ChangeTracking]    Script Date: 02/12/2025 16:41:06 ******/
DROP TRIGGER IF EXISTS [deb].[StatementRequirementScope_ChangeTracking]
GO

/****** Object:  Trigger [deb].[StatementRequirementScope_ChangeTracking]    Script Date: 02/12/2025 16:41:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Mark Seymour
-- Create date: 02/12/25
-- Description:	Change Tracking for the Many to Many StatementRequirementScope table
-- =============================================
CREATE TRIGGER [deb].[StatementRequirementScope_ChangeTracking] 
   ON  [deb].[StatementRequirementScope]
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

	SELECT @TableName = 'StatementRequirementScope'
	SELECT @FriendlyTableName = 'Requirement/Scope'

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
		-- Create a ChangeRecord for the event
		SELECT @sql = 'INSERT INTO common.ChangeRecord(EntityId, ChangeDate, Comments, ChangeByUser, IsDeleted, EventId)'
		SELECT @sql = @sql + ' SELECT TOP 1 ISNULL(i.StatementId, d.StatementId)'
		SELECT @sql = @sql + ', GETDATE()'
		SELECT @sql = @sql + ', ''Requirements/Scopes Changed'''
		SELECT @sql = @sql + ', ''' + @Username + ''''
		SELECT @sql = @sql + ', 0'
		SELECT @sql = @sql + ', ''' + CAST(@ChangeEventId AS NVARCHAR(72)) + ''''
		SELECT @sql = @sql + ' FROM #ins i FULL OUTER JOIN #del d'
		SELECT @sql = @sql + @PKCols
		SELECT @sql = @sql + ' SELECT @out_id = SCOPE_IDENTITY()'
		PRINT @sql
		EXEC sp_executeSQL @sql, N'@out_id INT OUTPUT', @out_id = @ChangeRecordId OUTPUT
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
			SELECT @ChangedFrom = STRING_AGG(CONCAT('Requirement: ', ehReq.SerialNumber, ', Scope: ', ehScope.Title), '; ')
			FROM deb.StatementRequirementScope srs
			JOIN common.ChangeRecord cr ON cr.EntityId = srs.StatementId
			JOIN common.EntityHead ehReq ON ehReq.EntityId = srs.RequirementId
			JOIN common.EntityHead ehScope ON ehScope.EntityId = srs.ScopeId
			LEFT JOIN INSERTED i ON i.StatementId = srs.StatementId AND i.RequirementId = srs.RequirementId AND i.ScopeId = srs.ScopeId
			WHERE cr.Id = @ChangeRecordId
			AND i.RequirementId IS NULL AND i.ScopeId IS NULL
		END
		ELSE
		BEGIN
			;WITH AllSRSs AS (
				SELECT * FROM deb.StatementRequirementScope srs
				UNION ALL
				SELECT * FROM DELETED
			)
			SELECT @ChangedFrom = STRING_AGG(CONCAT('Requirement: ', ehReq.SerialNumber, ', Scope: ', ehScope.Title), '; ')
			FROM AllSRSs srs
			JOIN common.ChangeRecord cr ON cr.EntityId = srs.StatementId
			JOIN common.EntityHead ehReq ON ehReq.EntityId = srs.RequirementId
			JOIN common.EntityHead ehScope ON ehScope.EntityId = srs.ScopeId
			WHERE cr.Id = @ChangeRecordId
		END

		SELECT @ChangedTo = STRING_AGG(CONCAT('Requirement: ', ehReq.SerialNumber, ', Scope: ', ehScope.Title), '; ')
		FROM deb.StatementRequirementScope srs
		JOIN common.ChangeRecord cr ON cr.EntityId = srs.StatementId
		JOIN common.EntityHead ehReq ON ehReq.EntityId = srs.RequirementId
		JOIN common.EntityHead ehScope ON ehScope.EntityId = srs.ScopeId
		WHERE cr.Id = @ChangeRecordId

		INSERT INTO common.ChangeRecordItem(ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
		SELECT @ChangeRecordId, @TableName, @FriendlyTableName, @ChangedFrom, @ChangedTo, 0
	END
	ELSE
	BEGIN
		SELECT @ChangedTo = STRING_AGG(CONCAT('Requirement: ', ehReq.SerialNumber, ', Scope: ', ehScope.Title), '; ')
		FROM deb.StatementRequirementScope srs
		JOIN common.ChangeRecord cr ON cr.EntityId = srs.StatementId
		JOIN common.EntityHead ehReq ON ehReq.EntityId = srs.RequirementId
		JOIN common.EntityHead ehScope ON ehScope.EntityId = srs.ScopeId
		WHERE cr.Id = @ChangeRecordId

		UPDATE common.ChangeRecordItem
		SET ChangedTo = @ChangedTo
		WHERE Id = @ChangeRecordItemId
	END
END
GO

ALTER TABLE [deb].[StatementRequirementScope] ENABLE TRIGGER [StatementRequirementScope_ChangeTracking]
GO
");
			#endregion
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"
				ALTER TABLE [deb].[StatementRequirementScope] DISABLE TRIGGER [StatementRequirementScope_ChangeTracking]
				DROP TRIGGER IF EXISTS [deb].[StatementRequirementScope_ChangeTracking]
            ");
		}
    }
}
