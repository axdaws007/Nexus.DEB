using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SmallFixForCreateChangeRecordItemSP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
/****** Object:  StoredProcedure [dbo].[CreateChangeRecordItem]    Script Date: 07/01/2026 09:37:54 ******/
DROP PROCEDURE IF EXISTS [dbo].[CreateChangeRecordItem]
GO

/****** Object:  StoredProcedure [dbo].[CreateChangeRecordItem]    Script Date: 07/01/2026 09:37:54 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreateChangeRecordItem]
	@entityId UNIQUEIDENTIFIER,
	@fieldName NVARCHAR(MAX),
	@friendlyFieldName NVARCHAR(MAX),
	@oldValue NVARCHAR(MAX),
	@newValue NVARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @ChangeEventId UNIQUEIDENTIFIER,
			@ChangeRecordId INT,
			@Username NVARCHAR(128)

	SELECT @ChangeEventId = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'EventId'))
	IF @ChangeEventId IS NULL
	BEGIN
		-- fallback (e.g. manual SQL scripts, ad hoc changes)
		SET @ChangeEventId = NEWID()
	END

	IF EXISTS(Select 1 From common.ChangeRecord Where EventId = @ChangeEventId)
	BEGIN
		IF EXISTS(Select 1 From common.ChangeRecord Where EventId = @ChangeEventId AND EntityId != @entityId)
		BEGIN
			-- ChangeEventId is pointing at wrong Entity, so we'll create a new parent record. (Should never happen in practice, but just in case)
			SELECT @ChangeEventId = NEWID()
			SELECT @UserName = dbo.Audit_GetUserContext()

			INSERT INTO common.ChangeRecord(EntityId, ChangeDate, Comments, ChangeByUser, IsDeleted, EventId)
			SELECT @entityId, GETDATE(), 'State updated', @Username, 0, @ChangeEventId

			SELECT @ChangeRecordId = SCOPE_IDENTITY()
		END
		ELSE
		BEGIN
			SELECT @ChangeRecordId = Id FROM common.ChangeRecord WHERE EventId = @ChangeEventId
		END
	END
	ELSE
	BEGIN
		SELECT @UserName = dbo.Audit_GetUserContext()
		
		INSERT INTO common.ChangeRecord(EntityId, ChangeDate, Comments, ChangeByUser, IsDeleted, EventId)
		SELECT @entityId, GETDATE(), 'State updated', @Username, 0, @ChangeEventId

		SELECT @ChangeRecordId = SCOPE_IDENTITY()
	END

	INSERT INTO common.ChangeRecordItem(ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
	SELECT @ChangeRecordId, @fieldName, @friendlyFieldName, @oldValue, @newValue, 0

	RETURN 0
END

GO
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"
/****** Object:  StoredProcedure [dbo].[CreateChangeRecordItem]    Script Date: 06/01/2026 14:54:02 ******/
DROP PROCEDURE IF EXISTS [dbo].[CreateChangeRecordItem]
GO

/****** Object:  StoredProcedure [dbo].[CreateChangeRecordItem]    Script Date: 06/01/2026 14:54:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




/*
 * This SP is designed to be run purely from within a trigger where the #ins and #del temp tables are created.
 * If you wish to create a ChangeRecord from any other context, then do NOT use this SP.
*/
CREATE PROCEDURE [dbo].[CreateChangeRecordItem]
	@entityId UNIQUEIDENTIFIER,
	@fieldName NVARCHAR(MAX),
	@friendlyFieldName NVARCHAR(MAX),
	@oldValue NVARCHAR(MAX),
	@newValue NVARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @ChangeEventId UNIQUEIDENTIFIER,
			@ChangeRecordId INT,
			@Username NVARCHAR(128)

	SELECT @ChangeEventId = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'EventId'))
	IF @ChangeEventId IS NULL
	BEGIN
		-- fallback (e.g. manual SQL scripts, ad hoc changes)
		SET @ChangeEventId = NEWID()
	END

	IF EXISTS(Select 1 From common.ChangeRecord Where EventId = @ChangeEventId)
	BEGIN
		IF EXISTS(Select 1 From common.ChangeRecord Where EventId = @ChangeEventId AND EntityId != @entityId)
		BEGIN
			-- ChangeEventId is pointing at wrong Entity, so we'll create a new parent record. (Should never happen in practice, but just in case)
			SELECT @ChangeEventId = NEWID()
			SELECT @UserName = dbo.Audit_GetUserContext()

			INSERT INTO common.ChangeRecord(EntityId, ChangeDate, Comments, ChangeByUser, IsDeleted, EventId)
			SELECT @entityId, GETDATE(), 'State updated', @Username, 0, @ChangeEventId

			SELECT @ChangeRecordId = SCOPE_IDENTITY()
		END
		ELSE
		BEGIN
			SELECT @ChangeRecordId = Id FROM common.ChangeRecord WHERE EventId = @ChangeEventId
		END
	END
	ELSE
	BEGIN
		SELECT @UserName = dbo.Audit_GetUserContext()
		
		INSERT INTO common.ChangeRecord(EntityId, ChangeDate, Comments, ChangeByUser, IsDeleted, EventId)
		SELECT @entityId, GETDATE(), 'State updated', @Username, 0, @ChangeEventId
	END

	INSERT INTO common.ChangeRecordItem(ChangeRecordId, FieldName, FriendlyFieldName, ChangedFrom, ChangedTo, IsDeleted)
	SELECT @ChangeRecordId, @fieldName, @friendlyFieldName, @oldValue, @newValue, 0

	RETURN 0
END

GO

");
		}
    }
}
