using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskDetailView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"

/****** Object:  View [deb].[vw_TaskDetail]    Script Date: 05/12/2025 13:32:34 ******/
DROP VIEW IF EXISTS [deb].[vw_TaskDetail]
GO

/****** Object:  View [deb].[vw_TaskDetail]    Script Date: 05/12/2025 13:32:34 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

                CREATE VIEW [deb].[vw_TaskDetail]
                AS
                SELECT 
	                eh.[EntityId],
	                eh.[Title],
	                eh.[Description],
	                eh.[SerialNumber],
	                eh.[CreatedDate],
	                eh.[LastModifiedDate],
	                eh.[IsRemoved],
	                eh.[IsArchived],
	                eh.[EntityTypeTitle],
	                t.[DueDate],
					tt.Id AS [TaskTypeId],
					tt.Title AS [TaskType],
	                vp_cr.[Title] AS [CreatedBy],
	                vp_lm.[Title] AS [LastModifiedBy],
	                vp_ow.[Title] AS [OwnedBy],
					eh.OwnedById,
					ped.ActivityID,
					ped.ActivityTitle AS [Status],
					ehs.EntityId AS [StatementId],
					ehs.Title AS [StatementTitle],
					ehs.SerialNumber AS [StatementSerialNumber]
                FROM [common].[EntityHead] eh
                INNER JOIN [deb].[Task] t ON eh.[EntityID] = t.[EntityID]
				INNER JOIN [deb].[TaskType] tt ON t.[TaskTypeId] = tt.[Id]
				INNER JOIN [deb].[Statement] s ON s.[EntityID] = t.[StatementId]
				LEFT JOIN common.vw_PawsEntityDetail ped ON eh.EntityID = ped.EntityID
                LEFT JOIN [common].[EntityHead] ehs ON s.[EntityID] = ehs.[EntityID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_cr ON eh.[CreatedById] = vp_cr.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_lm ON eh.[LastModifiedById] = vp_lm.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_ow ON eh.[OwnedById] = vp_ow.[ID]
            
GO
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS [deb].[vw_TaskDetail]
GO");
        }
    }
}
