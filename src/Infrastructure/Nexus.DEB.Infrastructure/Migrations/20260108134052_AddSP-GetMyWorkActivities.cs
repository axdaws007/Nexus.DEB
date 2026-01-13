using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSPGetMyWorkActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- =============================================
-- Author:		Sean Walsh
-- Create date: 2016-01-06
-- Description:	Gets the list of activities to be signed off given certain parameters.
-- Modified:	2016-09-29 - Added extra group filtering.
-- =============================================
CREATE PROCEDURE [common].[GetMyWorkActivities]
		
	@myPostID uniqueidentifier,		
	@selectedPostID uniqueidentifier,	
	@entityTypeTitle nvarchar(50),	
	@createdByOption int = 0,		-- 1 = Post, 2 = team
	@ownedByOption int = 0,			-- 1 = Post, 2 = team, 4 = group
	@progressedByOption int = 0,	-- 1 = Post, 2 = team, 3 = my roles
	@myRoles nvarchar(max) = NULL,  -- my roles as a CSV - only needed if @progressedByOptions = 3
	@csvTeamPostIDs nvarchar(max) = NULL,
	@csvGroupIDs nvarchar(max) = NULL
AS
BEGIN	
	SET NOCOUNT ON;
				
	DECLARE @tblMyRoles TABLE ([Value] UNIQUEIDENTIFIER)
	DECLARE @tblTeamPostIDs TABLE ([Value] UNIQUEIDENTIFIER)
	DECLARE @tblGroupIDs TABLE ([Value] UNIQUEIDENTIFIER)
	
	-- Do split now - might help performance		
	-- My Roles selected?
	IF @progressedByOption = 3 AND @myRoles IS NOT NULL
	BEGIN
		INSERT INTO @tblMyRoles
		SELECT CAST([Value] as UNIQUEIDENTIFIER) FROM dbo.Split(@myRoles,',')
	END
	
	-- Do split now - might help performance
	IF @csvTeamPostIDs IS NOT NULL
	BEGIN
		INSERT INTO @tblTeamPostIDs
		SELECT CAST([Value] as UNIQUEIDENTIFIER) FROM dbo.Split(@csvTeamPostIDs,',')
	END
	
	IF @csvGroupIDs IS NOT NULL AND @ownedByOption = 4
	BEGIN
		INSERT INTO @tblGroupIDs
		SELECT CAST([Value] as UNIQUEIDENTIFIER) FROM dbo.Split(@csvGroupIDs,',')
	END
			
	SELECT 	ActivityID, title as ActivityTitle, SUM(FormCount) as FormCount
	FROM
	(
		-- Main select
		SELECT activityID, title, FormCount 
		FROM
		(
			-- select actual data
			SELECT 					
				act.activityID,
				act.title,
				count(DISTINCT eh.EntityID) as FormCount
																																										
			FROM paws.PAWSEntityActivityStep peas	
			INNER JOIN common.EntityHead eh ON eh.EntityID = peas.EntityID	
			INNER JOIN common.DashboardInfo di ON di.EntityID = eh.EntityID	
			INNER JOIN paws.PAWSActivity act on act.activityID = peas.ActivityID			
			LEFT JOIN paws.PAWSEntityActivityOwner ao ON ao.EntityID = peas.EntityID AND ao.ActivityID = peas.ActivityID 
			LEFT JOIN PAWS.PAWSActivityOwnerRole actRole on actRole.activityID = peas.ActivityID
			
			WHERE	eh.EntityTypeTitle = @entityTypeTitle			
					AND PEAS.StatusID = 1						
					AND PEAS.IsActive = 1						
					AND eh.IsRemoved = 0
					AND eh.IsArchived = 0						
					AND (
							@createdByOption = 0 -- anyone
							OR 
							(@createdByOption = 1 AND eh.CreatedByID = @myPostID ) -- my post
							OR
							( @createdByOption = 2 AND eh.CreatedByID IN (SELECT * FROM @tblTeamPostIDs)  ) -- team
						)
					AND (
							@ownedByOption = 0 
							OR 
							(@ownedByOption = 1 AND eh.OwnedByID = @myPostID ) 	
							OR
							( @ownedByOption = 2 AND eh.OwnedByID IN (SELECT * FROM @tblTeamPostIDs)  ) -- team	ownership	
							OR
							( @ownedByOption = 4 AND eh.OwnedByGroupID IN (SELECT * FROM @tblGroupIDs)  ) -- group ownership		
						)																					
					AND ( -- PROGRESSED BY		
							@progressedByOption = 0
							OR					
							( @progressedByOption = 1 AND ao.OwnerID = @myPostID ) -- 	My Post			
							OR					
							( @progressedByOption = 2 AND ao.OwnerID = @selectedPostID ) -- Selected post from My Team										
							OR
							( @progressedByOption = 3 AND actRole.OwnerRoleID IN (SELECT * FROM @tblMyRoles)  ) -- My Roles
						)							
			GROUP BY act.activityID, act.title	
		) as actual		
	
	
	)  as tmp
	GROUP BY activityID, title
	ORDER BY FormCount DESC, title
											
END	
/*

exec [common].[GetMyWorkActivities] 'A7EC2C71-C363-4CEC-8813-04966A81976D', 'F791A', 0, 0, 1



	SELECT 		DISTINCT			
				act.activityID,
				act.title,
				count(DISTINCT eh.EntityID) as FormCount
																																										
			FROM paws.PAWSEntityActivityStep peas	
			INNER JOIN common.EntityHead eh ON eh.EntityID = peas.EntityID	
			INNER JOIN common.DashboardInfo di ON di.EntityID = eh.EntityID	
			RIGHT JOIN paws.PAWSActivity act on act.activityID = peas.ActivityID			
			LEFT JOIN paws.PAWSEntityActivityOwner ao ON ao.EntityID = peas.EntityID AND ao.ActivityID = peas.ActivityID 
			LEFT JOIN PAWS.PAWSActivityOwnerRole actRole on actRole.activityID = peas.ActivityID
			
			WHERE	act.processTemplateID = 'AC4F5927-4CCE-4219-A601-CB9A67607436'	
					AND PEAS.StatusID = 1	
					AND di.IsWorkflowActive = 1
					AND eh.IsRemoved = 0
					AND eh.IsArchived = 0		
					AND   ao.OwnerID = 'A7EC2C71-C363-4CEC-8813-04966A81976D'  -- 		
					
			GROUP BY act.activityID, act.title										
									
					
*/
			");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP PROCEDURE [common].[GetMyWorkActivities]
            ");
        }
    }
}
