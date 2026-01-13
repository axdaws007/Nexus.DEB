using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSPGetMyWorkSummaryDataForPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- =============================================
-- Author:		Sean Walsh
-- Create date: 2015-12-16
-- Description:	Gets the summary (count) data for the My Work report.
-- Modified:	2016-09-29 - Added extra group filtering.
-- =============================================
CREATE PROCEDURE [common].[GetMyWorkSummaryDataForPosts]
	
	@myPostID UNIQUEIDENTIFIER,
	@csvTeamPostIDs nvarchar(max) = NULL,
	@createdByOption int = 0,		-- 0 - not selected, 1 = Post, 2 = team
	@ownedByOption int = 0,			-- 0 - not selected, 1 = Post, 2 = team, 4 = group
	@progressedByOption int = 1,	-- 1 = Post, 2 = team, 3 = my roles
	@myRoles nvarchar(max) = NULL,  -- my roles as a CSV - only needed if @progressedByOptions = 3
	@csvGroupIDs nvarchar(max) = NULL
AS
BEGIN
	
	SET NOCOUNT ON;    
	
	DECLARE @tblTeamPostIDs TABLE ([Value] UNIQUEIDENTIFIER)
	DECLARE @tblMyRoles TABLE ([Value] UNIQUEIDENTIFIER)
	DECLARE @tblGroupIDs TABLE ([Value] UNIQUEIDENTIFIER)
	
	-- Do split now - might help performance
	IF @csvTeamPostIDs IS NOT NULL
	BEGIN
		INSERT INTO @tblTeamPostIDs
		SELECT CAST([Value] as UNIQUEIDENTIFIER) FROM dbo.Split(@csvTeamPostIDs,',')
	END
	
	-- My Roles selected?
	IF @progressedByOption = 3 AND @myRoles IS NOT NULL
	BEGIN
		INSERT INTO @tblMyRoles
		SELECT CAST([Value] as UNIQUEIDENTIFIER) FROM dbo.Split(@myRoles,',')
	END
	
	-- owned by group?
	IF @csvGroupIDs IS NOT NULL AND @ownedByOption = 4
	BEGIN
		INSERT INTO @tblGroupIDs
		SELECT CAST([Value] as UNIQUEIDENTIFIER) FROM dbo.Split(@csvGroupIDs,',')
	END
			
	;WITH MyWork
	AS
	(
		SELECT      -- When ""my roles"" is selected the post ID is no longer relevent
					(CASE WHEN @progressedByOption IN (0, 3)  THEN CAST(0x0 as uniqueidentifier) ELSE ISNULL(ao.OwnerID, CAST(0x0 as uniqueidentifier)) END) as PostID, 	
					(CASE WHEN @progressedByOption IN (0, 3) THEN ' ' ELSE ISNULL(vwp.Title, 'Unknown') END) as PostTitle,
					eh.EntityTypeTitle, 
					eh.EntityID
					
																															
		FROM common.DashboardInfo db 
		INNER JOIN common.EntityHead eh ON db.EntityID = eh.EntityID 	
		INNER JOIN PAWS.PAWSEntityActivityStep step on step.EntityID = eh.EntityID		
		LEFT JOIN PAWS.PAWSActivityOwnerRole actRole on actRole.activityID = step.ActivityID
		LEFT JOIN paws.PAWSEntityActivityOwner ao on ao.EntityID = step.EntityID AND ao.ActivityID =  step.ActivityID 
		LEFT JOIN [common].[XDB_CIS_View_Post] vwp ON ao.OwnerID = vwp.ID
		
		WHERE --db.IsWorkflowActive = 1 
			 
			eh.IsRemoved = 0
			AND eh.IsArchived = 0		
			AND step.StatusID = 1		
			AND step.IsActive = 1						
					--AND t.IsArchived	 = 0	
			AND (	-- CREATED BY
					@createdByOption = 0 -- not selected
					OR 
					( @createdByOption = 1 AND eh.CreatedByID = @myPostID ) -- post
					OR
					( @createdByOption = 2 AND eh.CreatedByID IN (SELECT * FROM @tblTeamPostIDs)  ) -- team
				)
			AND (  -- OWNED BY -- TODO - ######################################### owner???	
					@ownedByOption = 0 -- not selected
					OR 
					( @ownedByOption = 1 AND eh.OwnedByID = @myPostID ) -- post
					OR
					( @ownedByOption = 2 AND eh.OwnedByID IN (SELECT * FROM @tblTeamPostIDs)  ) -- team
					OR
					( @ownedByOption = 4 AND eh.OwnedByGroupID IN (SELECT * FROM @tblGroupIDs)  ) -- group ownership		
				)
			AND ( -- PROGRESSED BY
					(@progressedByOption = 0 ) --AND ao.OwnerType = 2) -- Any post
					OR 
					( @progressedByOption = 1 AND ao.OwnerID = @myPostID ) -- post
					OR
					( @progressedByOption = 2 AND ao.OwnerID IN (SELECT * FROM @tblTeamPostIDs)  ) -- team
					OR
					( @progressedByOption = 3 AND actRole.OwnerRoleID IN (SELECT * FROM @tblMyRoles)  ) -- My Roles
				)			
						    											        
				    							    											        
	)     
	   
	SELECT PostID, PostTitle, EntityTypeTitle, COUNT(DISTINCT EntityID) as FormCount
	FROM MyWork
	GROUP BY PostID, PostTitle, EntityTypeTitle
	--HAVING COUNT(EntityID) > 0
	ORDER BY PostTitle, EntityTypeTitle
	
END


/*

exec common.[GetMyWorkSummaryDataForPosts] 'A7EC2C71-C363-4CEC-8813-04966A81976D',NULL, 0, 0, 0, null, 'b2bde0e3-c5c9-4e0d-9df8-af0b18506fa3'

exec common.[GetMyWorkSummaryDataForPosts] 'A7EC2C71-C363-4CEC-8813-04966A81976D',NULL, 0, 0, 1

exec common.[GetMyWorkSummaryDataForPosts] 'A7EC2C71-C363-4CEC-8813-04966A81976D','9EF0C6F9-306E-482E-9D1E-2BE6D0DBD320', 0, 0, 1



exec common.[GetMyWorkSummaryDataForPosts] 'A7EC2C71-C363-4CEC-8813-04966A81976D',NULL, 0, 0, 3, 'fe3f2368-234f-440c-8471-00aadb01c1dd,6c2df04b-fa28-4a0b-87ca-0240dfc7c1bb,b2304af6-989e-44f6-9118-037e83a8eb90,f9756761-6db9-4986-b364-06332e0e9067,9dac778a-10fc-4a1b-a8cb-070476b36a8b,537885a3-60d5-41f6-8571-083fd1d89bcf,0916acf9-0a23-4c51-a44e-0b2ae6ea2305,cb12be54-b4ba-488e-a306-0cea7a2685ab,f55a36fa-ee4a-4789-8516-103920c37eb8,6170a2d7-d135-460e-8226-11b39b68a79a,3ebc307c-1fa3-4737-9253-1463b5b30cda,8a0d1c66-650e-4981-8953-15ac66df71ab,208ef91c-6540-4894-b541-15c2d3fcc029,2ff3f5e6-d1b6-4c40-9c5c-15d19c754fff,51ce5f79-f861-4dca-bc52-17aa9cb5aa3d,45b700cb-eb5c-4a69-bd81-1b018fc38221,d588c5b1-f137-4be3-83e9-1b206d2a2b58,9f3aeeac-f969-4ed7-a074-2068a605bb42,8e3f68a0-4c38-40a9-bb52-22850dff471a,e1521205-d820-4c3a-a1a0-22b8ad8e725d,2fd4aace-9b39-4ddc-a8e2-25333764a8eb,ba6a8d1d-8a4a-4f6f-9b27-254f5d1346e4,3cf49975-2bfc-4e39-9d09-25cb98fee9d0,3b732f2a-2121-4683-8a5a-261f2b3c71e2,66d5c16a-c1b9-4ed6-800f-285126a0032f,2ce03d5a-72a2-40b2-a3a4-289f33db8d38,686aec88-2d44-4168-a4d7-291a5f999ac8,4f867af5-4662-40cd-92a4-29216206923d,32bfd1b7-0ee1-4d6d-a2df-29404a0db9e9,c3b7a327-0a8a-455d-8702-296c2e2e4fa5,d498e7b6-d2ca-4887-a422-2aac21ac1277,9e9e727c-5fcb-4bc2-a9b0-2aee45bc22be,d8a45e26-2dbb-4d89-88af-2e6e95ea4670,1f43a620-7927-46dc-842e-2f267f1a8012,0afd0c73-90af-4668-b2bc-32f277f8b8f1,3773f3b6-4c83-41fa-b52b-33725cfcc1d5,9a7781f3-fafe-46c6-b882-348d85c9f4ed,f6e4152b-1f58-4c2b-883f-35dd8fb5821c,338b0176-4ac4-4ddd-907d-36e8bc18fe4f,1603993e-d9ac-49c4-8a99-380176d300b2,079681df-14ad-49c6-ab52-381deff7e6ae,2ddfe879-e1b3-41c1-9770-38aa8b911494,0f4b4bbd-4845-48b7-ad9b-39e8f521f028,2dc50f36-17a2-4a60-a4f2-3c5c357464da,f044e8d1-d843-401e-a81c-3cfbe46add64,99b0bfbe-2a83-492e-97e0-3d9ba293d94f,029ef0d2-a643-453c-94ab-3e9e01453b5e,41c268b3-6fa5-4687-b202-3f8479e52082,236a121d-663e-4617-ae82-4046b8581e5a,7572f319-e297-44d6-aca2-40cc8462b209,4e478345-5941-44c6-8a8c-41018de5113f,dd980ddd-d002-464b-b039-41f5a2a0d7db,913ddf04-12cc-4724-850f-44fff190b8ef,f542c3a3-c1ce-41b6-adec-46b5b9f5dd2f,c77d1094-f3d4-4385-b4fe-46eb88632c2c,64e69bfd-7743-4a39-bc9f-486d2c0c16b2,b0201b3c-7340-4b7d-aadb-4da913cd1acd,4e522d91-2763-4572-8a0c-4f759d9cc6d8,9093c903-3a00-4ec8-91df-578292827033,b87b69d0-c5d8-423a-984a-59760b6a0da6,558e242f-a552-490a-9281-63d3b3194b14,ee89a9dd-013e-4bd8-b292-6625b5c8a7dd,8eb5bc0f-cf8a-4a84-9b78-672722668236,e82124a7-67e7-4373-9d89-676ccbf9b975,c9f09d52-1972-4dd1-b8c6-6a6735e65272,8355510e-db7c-4744-b476-6dd4411ef511,ef91b595-0e5c-4842-8d54-70372acf68e2,c3712dc8-762c-4038-871e-758f4629febf,cd080d97-de00-458e-861b-75f32a3511af,6328a01c-0086-4527-9e03-76d2df85b8bd,db89e25a-7cea-4988-882c-771e9c843249,c4e261b5-d146-4aac-bd13-776cf3f6158d,11e2e83b-6e5c-46e3-b14b-786917d8e69a,2796e2ca-63e4-4ba0-a2e3-7996b7121bf6,8aa93ee7-0a75-4c37-8bb1-7a1ce06917ac,1ce6c1b9-236b-4fc9-a9d3-7c604c2a8cd3,6591b825-f851-48fc-b554-7dc63c182769,94d3fbc5-8cc6-4fd3-aff5-8033092a53b4,fb0a46c0-3efd-4920-870a-815630d092ba,461703d1-747f-4d71-ae7e-81d2724b72f8,8f543738-0989-4ef4-9eea-83377a6a66b3,3700abac-e72e-4e63-8767-8463e2abe2ca,f7bc4f19-8e98-4eea-a9ec-867aa4cdc9dc,da58702a-bf4f-431d-b4a8-8990b56bfbd6,64531128-3777-4b42-a416-8a99b08f75c2,dd151129-3e87-4eef-95ab-8b6ed32d3292,c04628bf-79fd-475b-8fde-8cb86eb843da,30489f39-5c31-4cf3-be64-8d5f7b14b596,6ed820d3-4edf-4ebf-9d56-8efe3fdbae28,c539f414-24b5-4593-9191-90f8cd0151f3,f603412a-94ef-4a77-b7e5-913f6fdb5ac0,3c0e09ae-e685-4f75-a462-954293fa4dc9,1f340dcb-f424-4650-9cef-959cba4b217b,7bc223b8-88cf-486c-af46-968eceae63c3,036796f2-88c1-4bb2-9edf-97ab5e55ee0f,9b4a393a-d2d0-4970-8cdc-9b45b8578512,c7e8c7ba-fd95-4e9e-8bfd-9d5b40aa614f,6af9bc88-9782-44e7-97f4-9f5de5b39988,33bd8150-cc22-4a10-a44c-a0ce6d5e0a01,d1f5c0cb-f73b-4239-ae7d-a41f1de855ce,3231ba7a-002d-445f-89e7-a60a709a8d13,647202ad-9257-4da3-b7f0-a74fd58a2353,a2073e1a-f2f7-4675-a34c-a7c72b31f918,a79e2901-a53f-4b47-9450-ad6a2d6dc65c,86ae8072-5e57-46a6-854f-aed655cc4332,08f3ba27-15ec-4e81-8383-af47a9d34c7a,01213264-36a8-40b2-a657-b195f28693ba,77807ced-4073-4510-8de8-b3181a67e7c8,d134c582-0079-4810-b62d-b3b1a968a2f9,c4819573-8b87-4d3f-a8cf-b3d92911a102,bcb2e006-c220-4edf-94db-b4522593ea14,d1955a26-fd80-4065-9245-b567d4ad1971,d184796d-a16d-412d-85be-bac6be69109d,012d5b95-1de0-4f8d-af11-bbf4a0aae8c2,956ccbb8-9ad1-4455-986f-bd16650c6dea,82fdd32c-06ff-4c1e-9ac2-bdee2345ccb0,66433af5-5700-4006-8512-bea316e58bab,8497e0b1-61c5-4530-ab5e-c08247db3a7c,3dc930c0-9400-45f2-883d-c13ac6883743,c5d88a03-1984-4393-b5ee-c3aa24262656,eed79009-f0f9-43d1-992e-c73b6e9675ab,8bb18005-9c25-4903-859f-ccb0ebb38226,b2903c01-a220-419b-a0bb-cdd1ab2d2cb1,19644b71-2ebe-4e01-a560-cf6d6c3dd163,7e680001-b13e-425c-8e24-d2a661c63e1d,da404c2c-706b-4abd-b4c5-d372026c1b86,848934c2-91e7-41ab-8fe3-d4e0a7ec7aa3,317c590d-5861-4a91-b4c8-d65953088e03,04d65bce-b1a6-4a56-8437-d937be4ec38a,e30fb94c-d54c-4615-83bb-da172804d158,c680ce60-045e-4cb2-ab77-da8f512e4f7b,60b48c55-265f-4972-80fb-dbf5af3e7721,3ae9c4e0-b31e-49e3-b995-dddb2ef081ef,7b02da56-cfc9-4e81-bd09-e51f158cc3db,eba567c6-0110-4119-bb1a-e72054e979bd,cf111304-bdd7-4c3d-b9f1-e8d378296f6d,5c434940-15c4-4a8e-b2e3-ef4d4de487d0,1f66238d-5b5b-425e-b024-f2e3702907bb,a9bb4ac0-8d33-4bc9-bc02-f37bdbf8f6c8,317528b1-e8f7-480a-bc9c-f511f42d03c1,89a376e2-f09f-4c10-8e73-fb088bb39b7e,947435ce-f72e-45a3-9e28-fd4a30281929,b1dfab7c-8379-4830-8f71-ff33e1fd0224,477f89a2-fbe2-4925-bc5b-ff6bb8eaa3a6'




SELECT    DISTINCT  
					eh.EntityTypeTitle, 
					eh.EntityID
					--act.processTemplateID as WorkflowID
																															
		FROM common.DashboardInfo db 
		INNER JOIN common.EntityHead eh ON db.EntityID = eh.EntityID 	
		INNER JOIN PAWS.PAWSEntityActivityStep step on step.EntityID = eh.EntityID
		INNER JOIN PAWS.PAWSActivity act on act.activityID = step.ActivityID
		LEFT JOIN PAWS.PAWSActivityOwnerRole actRole on actRole.activityID = step.ActivityID
		--LEFT JOIN paws.PAWSEntityActivityOwner ao on ao.EntityID = step.EntityID AND ao.ActivityID =  step.ActivityID AND step.StatusID = 1
		--LEFT JOIN [common].[vwCIS_View_Post] vwp ON ao.OwnerID = vwp.ID
		
		WHERE 
			 
			eh.IsRemoved = 0
			AND eh.IsArchived = 0		
			AND step.StatusID = 1		
			AND eh.EntityTypeTitle = 'issue'		
		
			AND 
					
					 actRole.OwnerRoleID IN ('fe3f2368-234f-440c-8471-00aadb01c1dd,6c2df04b-fa28-4a0b-87ca-0240dfc7c1bb,b2304af6-989e-44f6-9118-037e83a8eb90,f9756761-6db9-4986-b364-06332e0e9067,9dac778a-10fc-4a1b-a8cb-070476b36a8b,537885a3-60d5-41f6-8571-083fd1d89bcf,0916acf9-0a23-4c51-a44e-0b2ae6ea2305,cb12be54-b4ba-488e-a306-0cea7a2685ab,f55a36fa-ee4a-4789-8516-103920c37eb8,6170a2d7-d135-460e-8226-11b39b68a79a,3ebc307c-1fa3-4737-9253-1463b5b30cda,8a0d1c66-650e-4981-8953-15ac66df71ab,208ef91c-6540-4894-b541-15c2d3fcc029,2ff3f5e6-d1b6-4c40-9c5c-15d19c754fff,51ce5f79-f861-4dca-bc52-17aa9cb5aa3d,45b700cb-eb5c-4a69-bd81-1b018fc38221,d588c5b1-f137-4be3-83e9-1b206d2a2b58,9f3aeeac-f969-4ed7-a074-2068a605bb42,8e3f68a0-4c38-40a9-bb52-22850dff471a,e1521205-d820-4c3a-a1a0-22b8ad8e725d,2fd4aace-9b39-4ddc-a8e2-25333764a8eb,ba6a8d1d-8a4a-4f6f-9b27-254f5d1346e4,3cf49975-2bfc-4e39-9d09-25cb98fee9d0,3b732f2a-2121-4683-8a5a-261f2b3c71e2,66d5c16a-c1b9-4ed6-800f-285126a0032f,2ce03d5a-72a2-40b2-a3a4-289f33db8d38,686aec88-2d44-4168-a4d7-291a5f999ac8,4f867af5-4662-40cd-92a4-29216206923d,32bfd1b7-0ee1-4d6d-a2df-29404a0db9e9,c3b7a327-0a8a-455d-8702-296c2e2e4fa5,d498e7b6-d2ca-4887-a422-2aac21ac1277,9e9e727c-5fcb-4bc2-a9b0-2aee45bc22be,d8a45e26-2dbb-4d89-88af-2e6e95ea4670,1f43a620-7927-46dc-842e-2f267f1a8012,0afd0c73-90af-4668-b2bc-32f277f8b8f1,3773f3b6-4c83-41fa-b52b-33725cfcc1d5,9a7781f3-fafe-46c6-b882-348d85c9f4ed,f6e4152b-1f58-4c2b-883f-35dd8fb5821c,338b0176-4ac4-4ddd-907d-36e8bc18fe4f,1603993e-d9ac-49c4-8a99-380176d300b2,079681df-14ad-49c6-ab52-381deff7e6ae,2ddfe879-e1b3-41c1-9770-38aa8b911494,0f4b4bbd-4845-48b7-ad9b-39e8f521f028,2dc50f36-17a2-4a60-a4f2-3c5c357464da,f044e8d1-d843-401e-a81c-3cfbe46add64,99b0bfbe-2a83-492e-97e0-3d9ba293d94f,029ef0d2-a643-453c-94ab-3e9e01453b5e,41c268b3-6fa5-4687-b202-3f8479e52082,236a121d-663e-4617-ae82-4046b8581e5a,7572f319-e297-44d6-aca2-40cc8462b209,4e478345-5941-44c6-8a8c-41018de5113f,dd980ddd-d002-464b-b039-41f5a2a0d7db,913ddf04-12cc-4724-850f-44fff190b8ef,f542c3a3-c1ce-41b6-adec-46b5b9f5dd2f,c77d1094-f3d4-4385-b4fe-46eb88632c2c,64e69bfd-7743-4a39-bc9f-486d2c0c16b2,b0201b3c-7340-4b7d-aadb-4da913cd1acd,4e522d91-2763-4572-8a0c-4f759d9cc6d8,9093c903-3a00-4ec8-91df-578292827033,b87b69d0-c5d8-423a-984a-59760b6a0da6,558e242f-a552-490a-9281-63d3b3194b14,ee89a9dd-013e-4bd8-b292-6625b5c8a7dd,8eb5bc0f-cf8a-4a84-9b78-672722668236,e82124a7-67e7-4373-9d89-676ccbf9b975,c9f09d52-1972-4dd1-b8c6-6a6735e65272,8355510e-db7c-4744-b476-6dd4411ef511,ef91b595-0e5c-4842-8d54-70372acf68e2,c3712dc8-762c-4038-871e-758f4629febf,cd080d97-de00-458e-861b-75f32a3511af,6328a01c-0086-4527-9e03-76d2df85b8bd,db89e25a-7cea-4988-882c-771e9c843249,c4e261b5-d146-4aac-bd13-776cf3f6158d,11e2e83b-6e5c-46e3-b14b-786917d8e69a,2796e2ca-63e4-4ba0-a2e3-7996b7121bf6,8aa93ee7-0a75-4c37-8bb1-7a1ce06917ac,1ce6c1b9-236b-4fc9-a9d3-7c604c2a8cd3,6591b825-f851-48fc-b554-7dc63c182769,94d3fbc5-8cc6-4fd3-aff5-8033092a53b4,fb0a46c0-3efd-4920-870a-815630d092ba,461703d1-747f-4d71-ae7e-81d2724b72f8,8f543738-0989-4ef4-9eea-83377a6a66b3,3700abac-e72e-4e63-8767-8463e2abe2ca,f7bc4f19-8e98-4eea-a9ec-867aa4cdc9dc,da58702a-bf4f-431d-b4a8-8990b56bfbd6,64531128-3777-4b42-a416-8a99b08f75c2,dd151129-3e87-4eef-95ab-8b6ed32d3292,c04628bf-79fd-475b-8fde-8cb86eb843da,30489f39-5c31-4cf3-be64-8d5f7b14b596,6ed820d3-4edf-4ebf-9d56-8efe3fdbae28,c539f414-24b5-4593-9191-90f8cd0151f3,f603412a-94ef-4a77-b7e5-913f6fdb5ac0,3c0e09ae-e685-4f75-a462-954293fa4dc9,1f340dcb-f424-4650-9cef-959cba4b217b,7bc223b8-88cf-486c-af46-968eceae63c3,036796f2-88c1-4bb2-9edf-97ab5e55ee0f,9b4a393a-d2d0-4970-8cdc-9b45b8578512,c7e8c7ba-fd95-4e9e-8bfd-9d5b40aa614f,6af9bc88-9782-44e7-97f4-9f5de5b39988,33bd8150-cc22-4a10-a44c-a0ce6d5e0a01,d1f5c0cb-f73b-4239-ae7d-a41f1de855ce,3231ba7a-002d-445f-89e7-a60a709a8d13,647202ad-9257-4da3-b7f0-a74fd58a2353,a2073e1a-f2f7-4675-a34c-a7c72b31f918,a79e2901-a53f-4b47-9450-ad6a2d6dc65c,86ae8072-5e57-46a6-854f-aed655cc4332,08f3ba27-15ec-4e81-8383-af47a9d34c7a,01213264-36a8-40b2-a657-b195f28693ba,77807ced-4073-4510-8de8-b3181a67e7c8,d134c582-0079-4810-b62d-b3b1a968a2f9,c4819573-8b87-4d3f-a8cf-b3d92911a102,bcb2e006-c220-4edf-94db-b4522593ea14,d1955a26-fd80-4065-9245-b567d4ad1971,d184796d-a16d-412d-85be-bac6be69109d,012d5b95-1de0-4f8d-af11-bbf4a0aae8c2,956ccbb8-9ad1-4455-986f-bd16650c6dea,82fdd32c-06ff-4c1e-9ac2-bdee2345ccb0,66433af5-5700-4006-8512-bea316e58bab,8497e0b1-61c5-4530-ab5e-c08247db3a7c,3dc930c0-9400-45f2-883d-c13ac6883743,c5d88a03-1984-4393-b5ee-c3aa24262656,eed79009-f0f9-43d1-992e-c73b6e9675ab,8bb18005-9c25-4903-859f-ccb0ebb38226,b2903c01-a220-419b-a0bb-cdd1ab2d2cb1,19644b71-2ebe-4e01-a560-cf6d6c3dd163,7e680001-b13e-425c-8e24-d2a661c63e1d,da404c2c-706b-4abd-b4c5-d372026c1b86,848934c2-91e7-41ab-8fe3-d4e0a7ec7aa3,317c590d-5861-4a91-b4c8-d65953088e03,04d65bce-b1a6-4a56-8437-d937be4ec38a,e30fb94c-d54c-4615-83bb-da172804d158,c680ce60-045e-4cb2-ab77-da8f512e4f7b,60b48c55-265f-4972-80fb-dbf5af3e7721,3ae9c4e0-b31e-49e3-b995-dddb2ef081ef,7b02da56-cfc9-4e81-bd09-e51f158cc3db,eba567c6-0110-4119-bb1a-e72054e979bd,cf111304-bdd7-4c3d-b9f1-e8d378296f6d,5c434940-15c4-4a8e-b2e3-ef4d4de487d0,1f66238d-5b5b-425e-b024-f2e3702907bb,a9bb4ac0-8d33-4bc9-bc02-f37bdbf8f6c8,317528b1-e8f7-480a-bc9c-f511f42d03c1,89a376e2-f09f-4c10-8e73-fb088bb39b7e,947435ce-f72e-45a3-9e28-fd4a30281929,b1dfab7c-8379-4830-8f71-ff33e1fd0224,477f89a2-fbe2-4925-bc5b-ff6bb8eaa3a6')   -- My Roles
					
				
*/
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
				DROP PROCEDURE [common].[GetMyWorkSummaryDataForPosts]
            ");
        }
    }
}
