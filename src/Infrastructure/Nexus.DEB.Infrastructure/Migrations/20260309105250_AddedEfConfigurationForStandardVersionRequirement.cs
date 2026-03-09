using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedEfConfigurationForStandardVersionRequirement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER [deb].[StandardVersionRequirement_ChangeTracking]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER       TRIGGER [deb].[StandardVersionRequirement_ChangeTracking] 
   ON  [deb].[StandardVersionRequirement]
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

	SELECT @TableName = 'StandardVersionRequirement'
	SELECT @FriendlyTableName = 'StandardVersion / Requirement'

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
		EXEC dbo.CreateChangeRecordWithinTrigger  @PKValueSelect = 'ISNULL(i.StandardVersionId, d.RequirementId)', 
																	@PkCols = @PkCols, 
																	@Comments = 'StandardVersions/Requirements Changed',
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
			SELECT @ChangedFrom = STRING_AGG(CONCAT('StandardVersion: ', ehStandardVersion.Title, ', Requirement: ', ehReq.SerialNumber), '; ')
			FROM deb.StandardVersionRequirement sr
			JOIN common.ChangeRecord cr ON cr.EntityId = sr.StandardVersionId
			JOIN common.EntityHead ehReq ON ehReq.EntityId = sr.RequirementId
			JOIN common.EntityHead ehStandardVersion ON ehStandardVersion.EntityId = sr.StandardVersionId
			LEFT JOIN INSERTED i ON i.StandardVersionId = sr.StandardVersionId AND i.RequirementId = sr.RequirementId
			WHERE cr.Id = @ChangeRecordId
			AND i.RequirementId IS NULL AND i.StandardVersionId IS NULL
		END
		ELSE
		BEGIN
			;WITH AllSRs AS (
				SELECT * FROM deb.StandardVersionRequirement sr
				UNION ALL
				SELECT * FROM DELETED
			)
			SELECT @ChangedFrom = STRING_AGG(CONCAT('StandardVersion: ', ehStandardVersion.Title, ', Requirement: ', ehReq.SerialNumber), '; ')
			FROM AllSRs sr
			JOIN common.ChangeRecord cr ON cr.EntityId = sr.StandardVersionId
			JOIN common.EntityHead ehReq ON ehReq.EntityId = sr.RequirementId
			JOIN common.EntityHead ehStandardVersion ON ehStandardVersion.EntityId = sr.StandardVersionId
			WHERE cr.Id = @ChangeRecordId
		END

		SELECT @ChangedTo = STRING_AGG(CONCAT('StandardVersion: ', ehStandardVersion.Title, ', Requirement: ', ehReq.SerialNumber), '; ')
		FROM deb.StandardVersionRequirement sr
		JOIN common.ChangeRecord cr ON cr.EntityId = sr.StandardVersionId
		JOIN common.EntityHead ehReq ON ehReq.EntityId = sr.RequirementId
		JOIN common.EntityHead ehStandardVersion ON ehStandardVersion.EntityId = sr.StandardVersionId
		WHERE cr.Id = @ChangeRecordId

		INSERT INTO common.ChangeRecordItem(ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
		SELECT @ChangeRecordId, @TableName, @FriendlyTableName, @ChangedFrom, @ChangedTo, 0
	END
	ELSE
	BEGIN
		SELECT @ChangedTo = STRING_AGG(CONCAT('StandardVersion: ', ehStandardVersion.Title, ', Requirement: ', ehReq.SerialNumber), '; ')
		FROM deb.StandardVersionRequirement sr
		JOIN common.ChangeRecord cr ON cr.EntityId = sr.StandardVersionId
		JOIN common.EntityHead ehReq ON ehReq.EntityId = sr.RequirementId
		JOIN common.EntityHead ehStandardVersion ON ehStandardVersion.EntityId = sr.StandardVersionId
		WHERE cr.Id = @ChangeRecordId

		UPDATE common.ChangeRecordItem
		SET ChangedTo = @ChangedTo
		WHERE Id = @ChangeRecordItemId
	END
END
			");
        }
    }
}
