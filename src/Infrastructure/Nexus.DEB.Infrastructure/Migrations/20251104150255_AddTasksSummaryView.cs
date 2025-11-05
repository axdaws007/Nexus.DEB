using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTasksSummaryView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW [deb].[vw_TaskSummary]
                AS
                SELECT
                    t.[EntityId],
                    eh.[SerialNumber],
                    eh.[Title],
					eh.[OwnedById],
					t.[DueDate],
					tt.[Title] AS [TaskTypeTitle],
					vw.[StateID] AS [StatusId],
					vw.[StateTitle] AS [Status]
                FROM [deb].[Task] t
                INNER JOIN [common].[EntityHead] eh ON t.[EntityId] = eh.[EntityId]
                INNER JOIN [deb].[TaskType] tt ON t.[TaskTypeId] = tt.[Id]
				LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP VIEW IF EXISTS [deb].[vw_TaskSummary];
            ");
        }
    }
}
