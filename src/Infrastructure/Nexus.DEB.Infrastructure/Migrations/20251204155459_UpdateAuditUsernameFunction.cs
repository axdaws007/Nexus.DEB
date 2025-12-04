using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuditUsernameFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			#region dbo.Audit_GetUserContext
			migrationBuilder.Sql(@"
                
/****** Object:  UserDefinedFunction [dbo].[Audit_GetUserContext]    Script Date: 04/12/2025 15:43:40 ******/
DROP FUNCTION IF EXISTS [dbo].[Audit_GetUserContext]
GO

/****** Object:  UserDefinedFunction [dbo].[Audit_GetUserContext]    Script Date: 04/12/2025 15:43:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		Mark Seymour
-- Create date: 2025-12-04
-- Description:	Updated to look for UserDetails for the Session context value UserDetails. 
-- If not found it falls back to Carbon versio of this function
-- =============================================
CREATE FUNCTION [dbo].[Audit_GetUserContext] ()
RETURNS VARCHAR(128)
AS
BEGIN

    DECLARE @UserName VARCHAR(250),
			@ContextVal VARCHAR(150)
    
	SELECT @UserName = TRY_CONVERT(nvarchar(250), SESSION_CONTEXT(N'UserDetails'))
	IF ISNULL(@UserName,'') = ''
	BEGIN
		SET @ContextVal = CONTEXT_INFO()
		IF PATINDEX('%{%}%', @ContextVal) > 0
		BEGIN
    
		   SET @ContextVal = LEFT (@ContextVal, PATINDEX('%{%', @ContextVal) -1) +
							 RIGHT(@ContextVal, DATALENGTH(@ContextVal) - PATINDEX('%}%', @ContextVal))       
    
		END
    
		SET @Username = REPLACE(COALESCE(CONVERT(VARCHAR(128), @ContextVal), SUSER_NAME()), '''','''''')     
	END
		
	RETURN @UserName
END
GO

            ");
			#endregion
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
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
		}
	}
}
