using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStatementViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StatementDetail]
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
	                st.[ReviewDate],
	                srs.[ScopeID],
	                ehsc.[Title] AS [Scope],
	                st.[StatementText],
	                vp_cr.[Title] AS [CreatedBy],
	                vp_lm.[Title] AS [LastModifiedBy],
	                vp_ow.[Title] AS [OwnedBy]
                FROM [common].[EntityHead] eh
                INNER JOIN [deb].[Statement] st ON eh.[EntityID] = st.[EntityID]
				INNER JOIN [deb].[StatementRequirementScope] srs ON st.[EntityID] = srs.[StatementId]
                LEFT JOIN [common].[EntityHead] ehsc ON srs.[ScopeID] = ehsc.[EntityID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_cr ON eh.[CreatedById] = vp_cr.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_lm ON eh.[LastModifiedById] = vp_lm.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_ow ON eh.[OwnedById] = vp_ow.[ID]
            ");

            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StatementExport]
                AS
                SELECT
                    st.[EntityId],
                    eh.[SerialNumber],
                    eh.[Title],
					eh.[Description],
                    eh.[LastModifiedDate],
					eh.[OwnedById],
    				vp.[Title] AS [OwnedBy],
                    (
                        SELECT STRING_AGG(ehr.SerialNumber, ', ')
                        FROM [common].[EntityHead] ehr
                        INNER JOIN [deb].[Requirement] r ON ehr.[EntityId] = r.[EntityId]
                        INNER JOIN [deb].[StatementRequirementScope] srs ON r.[EntityId] = srs.[RequirementId]
                        WHERE srs.[StatementId] = st.[EntityId]
                    ) AS RequirementSerialNumbers,
					vw.[StateID] AS [StatusId],
					vw.[StateTitle] AS [Status]
                FROM [deb].[Statement] st
                INNER JOIN [common].[EntityHead] eh ON st.[EntityId] = eh.[EntityId]
				LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
				LEFT JOIN [common].[XDB_CIS_View_Post] vp ON eh.[OwnedById] = vp.[ID]
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StatementDetail]
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
	                st.[ReviewDate],
	                st.[ScopeID],
	                ehsc.[Title] AS [Scope],
	                st.[StatementText],
	                vp_cr.[Title] AS [CreatedBy],
	                vp_lm.[Title] AS [LastModifiedBy],
	                vp_ow.[Title] AS [OwnedBy]
                FROM [common].[EntityHead] eh
                INNER JOIN [deb].[Statement] st ON eh.[EntityID] = st.[EntityID]
                LEFT JOIN [common].[EntityHead] ehsc ON st.[ScopeID] = ehsc.[EntityID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_cr ON eh.[CreatedById] = vp_cr.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_lm ON eh.[LastModifiedById] = vp_lm.[ID]
                LEFT JOIN [common].[XDB_CIS_View_Post] vp_ow ON eh.[OwnedById] = vp_ow.[ID]
            ");

            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StatementExport]
                AS
                SELECT
                    st.[EntityId],
                    eh.[SerialNumber],
                    eh.[Title],
					eh.[Description],
                    eh.[LastModifiedDate],
					eh.[OwnedById],
    				vp.[Title] AS [OwnedBy],
                    (
                        SELECT STRING_AGG(ehr.SerialNumber, ', ')
                        FROM [common].[EntityHead] ehr
                        INNER JOIN [deb].[Requirement] r ON ehr.[EntityId] = r.[EntityId]
                        INNER JOIN [deb].[StatementRequirement] str ON r.[EntityId] = str.[RequirementId]
                        WHERE str.[StatementId] = st.[EntityId]
                    ) AS RequirementSerialNumbers,
					vw.[StateID] AS [StatusId],
					vw.[StateTitle] AS [Status]
                FROM [deb].[Statement] st
                INNER JOIN [common].[EntityHead] eh ON st.[EntityId] = eh.[EntityId]
				LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
				LEFT JOIN [common].[XDB_CIS_View_Post] vp ON eh.[OwnedById] = vp.[ID]
            ");
        }
    }
}
