using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCisSynonymAndAlterTaskSummaryView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE SYNONYM [common].[XDB_CIS_View_Post] FOR [EDEV_Carbon_CIS].[dbo].[View_Post]
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
					tt.[Title] AS [TaskTypeTitle],
					vw.[StateID] AS [StatusId],
					vw.[StateTitle] AS [Status],
                    t.StatementId
                FROM [deb].[Task] t
                INNER JOIN [common].[EntityHead] eh ON t.[EntityId] = eh.[EntityId]
                INNER JOIN [deb].[TaskType] tt ON t.[TaskTypeId] = tt.[Id]
				LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
				LEFT JOIN [common].[XDB_CIS_View_Post] vp ON eh.[OwnedById] = eh.[OwnedById]
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
					t.[DueDate],
					tt.[Title] AS [TaskTypeTitle],
					vw.[StateID] AS [StatusId],
					vw.[StateTitle] AS [Status],
                    t.StatementId
                FROM [deb].[Task] t
                INNER JOIN [common].[EntityHead] eh ON t.[EntityId] = eh.[EntityId]
                INNER JOIN [deb].[TaskType] tt ON t.[TaskTypeId] = tt.[Id]
				LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
            ");
        }
    }
}
