using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPAWSEntityActivityStepTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"

/****** Object:  Trigger [PAWSEntityActivityStep_ChangeTracking]    Script Date: 16/12/2025 16:06:09 ******/
DROP TRIGGER IF EXISTS [paws].[PAWSEntityActivityStep_ChangeTracking]
GO

/****** Object:  Trigger [common].[PAWSEntityActivityStep_ChangeTracking]    Script Date: 16/12/2025 16:06:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE trigger [paws].[PAWSEntityActivityStep_ChangeTracking] on [paws].[PAWSEntityActivityStep] for  insert
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
	@EntityID UNIQUEIDENTIFIER,
	@ChangeEventId UNIQUEIDENTIFIER,
	@ChangeRecordId INT,
	@Comments NVARCHAR(MAX),
	@PreviousActivity NVARCHAR(100),
	@NewActivity NVARCHAR(100)

	-- Generic way of getting table name
    SELECT @TableName = (OBJECT_NAME( parent_id )) 
    FROM sys.triggers 
    WHERE object_id = @@PROCID

	-- date and user
    SELECT @UserName = dbo.Audit_GetUserContext()

	-- Action
    SELECT @Type = 'I'
	SELECT @Comments = 'State updated'

	-- get list of columns
    SELECT * INTO #ins FROM INSERTED
    SELECT * INTO #del FROM DELETED
	
	SELECT @EntityID = EntityID FROM #ins

	-- We are only tracking paws history for Tasks (PAWS Lite)
	IF NOT EXISTS(Select 1 From deb.Task Where EntityID = @EntityID)
	BEGIN
		RETURN
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
    SELECT @PKValueSelect = 'convert(varchar(100), i.EntityId)'

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


	SELECT TOP 1 @PreviousActivity = peas.Title
	FROM paws.PAWSEntityActivityStep peas
	LEFT JOIN #ins i ON peas.EntityActivityStepID = i.EntityActivityStepID
	WHERE peas.EntityID = @EntityID AND i.EntityActivityStepID IS NULL
	ORDER BY peas.EntityActivityStepID DESC

	SELECT TOP 1 @NewActivity = i.Title
	FROM #ins i

	INSERT INTO common.ChangeRecordItem(ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
	SELECT @ChangeRecordId, 'PendingActivity', 'State', @PreviousActivity, @NewActivity, 0
GO

ALTER TABLE [paws].[PAWSEntityActivityStep] ENABLE TRIGGER [PAWSEntityActivityStep_ChangeTracking]
GO

");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"
ALTER TABLE [paws].[PAWSEntityActivityStep] DISABLE TRIGGER [PAWSEntityActivityStep_ChangeTracking]
GO
DROP TRIGGER IF EXISTS [paws].[PAWSEntityActivityStep_ChangeTracking]
GO
");
		}
    }
}
