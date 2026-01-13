using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSPGetMyWorkDetailDataForPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- =============================================
-- Author:		Sean Walsh, modified by Alex Dawson for DEB
-- Create date: 2015-12-15
-- Description:	Used by the common ""My Work"" report.
-- Modified:	2016-09-29 - Added extra group filtering.
-- Modified:    2026-01-08 - For DEB
-- =============================================
CREATE PROCEDURE [common].[GetMyWorkDetailDataForPost]
	
	-- query args
	@myPostID uniqueidentifier,	
	@selectedPostID uniqueidentifier,	
	@entityTypeTitle nvarchar(50),		
	@createdByOption int = 0,		-- 1 = Post, 2 = team
	@ownedByOption int = 0,			-- 1 = Post, 2 = team, 4 = Groups
	@progressedByOption int = 0,	-- 1 = Post, 2 = team, 3 = my roles
	@myRoles nvarchar(max) = NULL,  -- my roles as a CSV - only needed if @progressedByOptions = 3
	@activityIDs nvarchar(max) = NULL,
	@csvTeamPostIDs nvarchar(max) = NULL,
	@csvGroupIDs nvarchar(max) = NULL,
	@CreatedStart datetime = NULL,
	@CreatedEnd datetime = NULL,
	@TransferStart datetime = NULL,
	@TransferEnd datetime = NULL,
		
	-- additional optional args - placed here to prevent exist code crashing
	@workflowID uniqueidentifier = NULL -- eg. F791A could have one of two
	
AS
BEGIN	
	SET NOCOUNT ON;
	
	SET ARITHABORT ON; -- Weirdly this stops timeout when called from web app.
		
	IF @activityIDs IS NULL 
	BEGIN
		SET @activityIDs = '';
	END;
	
	
	DECLARE @tblMyRoles TABLE ([Value] UNIQUEIDENTIFIER)
	DECLARE @tblActivityIDs TABLE ([Value] int)
	DECLARE @tblTeamPostIDs TABLE ([Value] UNIQUEIDENTIFIER)
	DECLARE @tblGroupIDs TABLE ([Value] UNIQUEIDENTIFIER)
		
	-- My Roles selected?
	IF @progressedByOption = 3 AND @myRoles IS NOT NULL
	BEGIN
		INSERT INTO @tblMyRoles
		SELECT CAST([Value] as UNIQUEIDENTIFIER) FROM dbo.Split(@myRoles,',')
	END
	
	INSERT INTO @tblActivityIDs
	SELECT CAST([Value] as int) FROM dbo.Split(@activityIDs,',')
	
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
		
				
	DECLARE  @tmpTA TABLE
	(
		ModuleID uniqueidentifier,
		EntityTypeTitle nvarchar(50),
		EntityID uniqueidentifier,
		SerialNumber nvarchar(150),
		Title nvarchar(max),
		CreatedDate DATETIME,
		ModifiedDate DATETIME,		
		DueDate DATETIME NULL,
		ReviewDate DATETIME NULL,
		PendingActivityList nvarchar(max) NULL,
		PendingActivityOwners nvarchar(max) NULL,
		OwnerGroup nvarchar(200) NULL,
		OwnerPost nvarchar(200) NULL,
		MaxTransferDate DATETIME NULL, -- the max of the created dates of paws pendings
		TransferDates nvarchar(max) NULL	-- csv of pending created dates		
	)
				
	-- insert into temp table.
	INSERT INTO @tmpTA			
	SELECT DISTINCT eh.ModuleID,
					eh.EntityTypeTitle,	
					eh.entityID, 					
					eh.SerialNumber, 
					eh.Title,
					eh.CreatedDate,
					eh.LastModifiedDate,
					db.DueDate,
					db.ReviewDate,
					-- Activity list
					stuff ((SELECT ',' + step.Title
							FROM paws.PAWSEntityActivityStep step
							LEFT JOIN paws.PAWSPseudoStateLookup psl ON psl.ActivityID = step.ActivityID AND psl.ActivityStatusID = 1							
							WHERE step.IsActive = 1  AND step.statusID = 1 AND step.EntityID = peas.entityID  
							ORDER BY psl.Rank ASC			
							FOR XML PATH(''), TYPE).value('.', 'VARCHAR(max)'), 1, 1, '') AS PendingActivityList,
					stuff ((SELECT ',' + isnull(post.Title ,'')
								FROM paws.PAWSEntityActivityStep step
								LEFT JOIN paws.PAWSPseudoStateLookup psl ON psl.ActivityID = step.ActivityID AND psl.ActivityStatusID = 1
								LEFT JOIN paws.PAWSEntityActivityOwner [owner]
									ON step.EntityID = [owner].EntityID AND step.ActivityID = [owner].ActivityID
								LEFT JOIN [common].[XDB_CIS_View_Post] post ON post.ID = [owner].OwnerID
								WHERE  step.IsActive = 1  AND step.statusID = 1 AND step.EntityID = peas.entityID
								ORDER BY psl.Rank ASC
								FOR XML PATH(''), TYPE).value('.', 'VARCHAR(max)'), 1, 1, '') AS PendingActivityOwners,
									
					grp.Name as OwnerGroup,
					ownerPost.Title as OwnerPost,
					MAX(peas.Created) OVER (PARTITION BY peas.entityID) AS MaxTransferDate,
					stuff ((SELECT ',' + convert(varchar(max), step.created, 103)
							FROM paws.PAWSEntityActivityStep step
							LEFT JOIN paws.PAWSPseudoStateLookup psl ON psl.ActivityID = step.ActivityID AND psl.ActivityStatusID = 1							
							WHERE step.IsActive = 1  AND step.statusID = 1 AND step.EntityID = peas.entityID  
							ORDER BY psl.Rank ASC			
							FOR XML PATH(''), TYPE).value('.', 'VARCHAR(max)'), 1, 1, '') AS TransferDates
					
					
																								
	FROM paws.PAWSEntityActivityStep peas	
	INNER JOIN common.EntityHead eh ON eh.EntityID = peas.EntityID		
	INNER JOIN common.DashboardInfo db ON eh.EntityID = db.EntityID	
	LEFT JOIN paws.PAWSEntityActivityOwner ao ON ao.EntityID = peas.EntityID AND ao.ActivityID = peas.ActivityID AND peas.StatusID = 1
	LEFT JOIN PAWS.PAWSActivityOwnerRole actRole on actRole.activityID = peas.ActivityID
	LEFT JOIN PAWS.PAWSActivity act on act.activityID = peas.ActivityID
	LEFT JOIN [common].[XDB_CIS_View_Group] grp ON grp.ID = eh.OwnedByGroupID
	LEFT JOIN [common].[XDB_CIS_View_Post] ownerPost ON ownerPost.ID = eh.OwnedByID
	WHERE	eh.EntityTypeTitle = @entityTypeTitle 			
			AND peas.StatusID = 1						
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
					( @ownedByOption = 2 AND eh.OwnedByID IN (SELECT * FROM @tblTeamPostIDs)  ) -- team		
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
			AND ( @activityIDs = '' OR peas.activityID IN (SELECT * FROM @tblActivityIDs) )		
			AND (@workflowID IS NULL OR act.processTemplateID = @workflowID)
			
			AND (@transferStart IS NULL OR (peas.created >= @transferStart))
			AND (@transferEnd IS NULL OR (peas.created < @transferEnd)) -- not <= !!!
		
			AND (@createdStart IS NULL OR (eh.CreatedDate >= @createdStart))
			AND (@createdEnd IS NULL OR (eh.CreatedDate < @createdEnd)) -- not <= !!!
											
				
	SELECT * FROM @tmpTA			 
END            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
				DROP PROCEDURE [common].[GetMyWorkDetailDataForPost]
            ");
        }
    }
}
