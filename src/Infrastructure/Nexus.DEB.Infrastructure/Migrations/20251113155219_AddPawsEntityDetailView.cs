using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPawsEntityDetailView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE  VIEW [common].[vw_PawsEntityDetail]
                AS
                SELECT	
	                eas.EntityID,	
					eas.EntityActivityStepID AS StepID,
	                eas.ActivityID,	
	                eas.Title AS ActivityTitle,	
	                eas.StatusID,   
	                pas.title as StatusTitle,	
	                ps.PseudoStateID,	
	                ps.PseudoStateTitle
                FROM paws.PAWSEntityActivityStep AS eas
                INNER JOIN paws.PAWSPseudoStateLookup AS psl ON eas.ActivityID = psl.ActivityID AND eas.StatusID = psl.ActivityStatusID
                INNER JOIN paws.PAWSPseudoState AS ps ON psl.PseudoStateID = ps.PseudoStateID
                INNER JOIN paws.PAWSActivityStatus pas ON eas.StatusID = pas.ActivityStatusID
                WHERE eas.IsActive = 1
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP VIEW IF EXISTS [common].[vw_PawsEntityDetail];
            ");
        }
    }
}
