using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionColumnToTaskSummaryView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_TaskSummary]
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
        }
    }
}
