using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalDueDateColumnToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "OriginalDueDate",
                schema: "deb",
                table: "Task",
                type: "date",
                nullable: true);

            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_TaskDetail]
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
                    t.[OriginalDueDate],
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
            ");

            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_TaskExport]
                AS
                SELECT
                    t.[EntityId],
                    eh.[SerialNumber],
                    eh.[Title],
                    eh.[Description],
					eh.[OwnedById],
					vp.Title AS [OwnedBy],
					t.[DueDate],
                    t.[OriginalDueDate],
                    tt.Id AS [TaskTypeId],
					tt.[Title] AS [TaskTypeTitle],
					vw.[StateID] AS [StatusId],
					vw.[StateTitle] AS [Status],
                    t.StatementId,
					eht.SerialNumber AS [StatementSerialNumber]
                FROM [deb].[Task] t
                INNER JOIN [common].[EntityHead] eh ON t.[EntityId] = eh.[EntityId]
                INNER JOIN [common].[EntityHead] eht ON t.[statementId] = eht.[EntityId]
                INNER JOIN [deb].[TaskType] tt ON t.[TaskTypeId] = tt.[Id]
				LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
				LEFT JOIN [common].[XDB_CIS_View_Post] vp ON eh.[OwnedById] = vp.[ID]
            ");

            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_TaskSummary]
                AS
                SELECT
                    t.[EntityId],
                    eh.[SerialNumber],
                    eh.[Title],
					eh.[OwnedById],
					vp.Title AS [OwnedBy],
					t.[DueDate],
                    t.[OriginalDueDate],
                    tt.Id as TaskTypeId,
					tt.[Title] AS [TaskTypeTitle],
					vw.[StateID] AS [StatusId],
					vw.[StateTitle] AS [Status],
                    t.StatementId,
                    eh.EntityTypeTitle
                FROM [deb].[Task] t
                INNER JOIN [common].[EntityHead] eh ON t.[EntityId] = eh.[EntityId]
                INNER JOIN [deb].[TaskType] tt ON t.[TaskTypeId] = tt.[Id]
				LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
				LEFT JOIN [common].[XDB_CIS_View_Post] vp ON eh.[OwnedById] = vp.[ID]
            ");

            migrationBuilder.Sql(@"
                UPDATE [deb].[Task]
                SET [OriginalDueDate] = [DueDate]
                WHERE [DueDate] IS NOT NULL
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_TaskSummary]
                AS
                SELECT
                    t.[EntityId],
                    eh.[SerialNumber],
                    eh.[Title],
					eh.[OwnedById],
					vp.Title AS [OwnedBy],
					t.[DueDate],
                    tt.Id as TaskTypeId,
					tt.[Title] AS [TaskTypeTitle],
					vw.[StateID] AS [StatusId],
					vw.[StateTitle] AS [Status],
                    t.StatementId,
                    eh.EntityTypeTitle
                FROM [deb].[Task] t
                INNER JOIN [common].[EntityHead] eh ON t.[EntityId] = eh.[EntityId]
                INNER JOIN [deb].[TaskType] tt ON t.[TaskTypeId] = tt.[Id]
				LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
				LEFT JOIN [common].[XDB_CIS_View_Post] vp ON eh.[OwnedById] = vp.[ID]
            ");

            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_TaskExport]
                AS
                SELECT
                    t.[EntityId],
                    eh.[SerialNumber],
                    eh.[Title],
                    eh.[Description],
					eh.[OwnedById],
					vp.Title AS [OwnedBy],
					t.[DueDate],
                    tt.Id AS [TaskTypeId],
					tt.[Title] AS [TaskTypeTitle],
					vw.[StateID] AS [StatusId],
					vw.[StateTitle] AS [Status],
                    t.StatementId,
					eht.SerialNumber AS [StatementSerialNumber]
                FROM [deb].[Task] t
                INNER JOIN [common].[EntityHead] eh ON t.[EntityId] = eh.[EntityId]
                INNER JOIN [common].[EntityHead] eht ON t.[statementId] = eht.[EntityId]
                INNER JOIN [deb].[TaskType] tt ON t.[TaskTypeId] = tt.[Id]
				LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
				LEFT JOIN [common].[XDB_CIS_View_Post] vp ON eh.[OwnedById] = vp.[ID]
            ");

            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_TaskDetail]
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
            ");

            migrationBuilder.DropColumn(
                name: "OriginalDueDate",
                schema: "deb",
                table: "Task");
        }
    }
}
