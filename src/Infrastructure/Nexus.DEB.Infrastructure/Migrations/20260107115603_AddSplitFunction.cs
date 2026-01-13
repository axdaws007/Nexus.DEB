using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSplitFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- =============================================
-- Author:		IM (from DEVX)
-- Create date: 23 Apr 2009
-- Description:	Like CLR String.Split()
-- =============================================
CREATE FUNCTION [dbo].[Split]
(
	@String		nvarchar (4000),
	@Delimiter	nvarchar (10)
)
RETURNS @ValueTable TABLE ([Value] nvarchar(4000))
BEGIN
	DECLARE @NextString nvarchar(4000)
	DECLARE @Pos int
	DECLARE @NextPos int
	DECLARE @CommaCheck nvarchar(1)

	IF @String IS NULL OR LEN(@String) = 0
		RETURN


	--Initialize
	SET @NextString = ''
	SET @CommaCheck = RIGHT(@String,1) 

	--Check for trailing Comma, if not exists, INSERT
	if (@CommaCheck <> @Delimiter )
		SET @String = @String + @Delimiter

	--Get position of first Comma
	SET @Pos = CHARINDEX(@Delimiter,@String)
	SET @NextPos = 1

	--Loop while there is still a comma in the String of levels
	WHILE (@pos <>  0)  
	BEGIN
		SET @NextString = SUBSTRING(@String,1,@Pos - 1)

		INSERT INTO @ValueTable ( [Value]) Values (@NextString)

		SET @String = SUBSTRING(@String,@pos +1,LEN(@String))
		SET @NextPos = @Pos
	SET @pos  = CHARINDEX(@Delimiter,@String)
	END

	RETURN
END
			");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP FUNCTION [dbo].[Split]
");
        }
    }
}
