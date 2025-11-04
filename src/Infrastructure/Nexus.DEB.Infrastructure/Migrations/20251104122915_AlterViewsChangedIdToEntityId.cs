using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlterViewsChangedIdToEntityId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StatementSummary]
                AS
                SELECT
                    st.[EntityId],
                    eh.[SerialNumber],
                    eh.[Title],
                    eh.[LastModifiedDate],
					eh.[OwnedById],
                    (
                        SELECT STRING_AGG(ehr.SerialNumber, ', ')
                        FROM [common].[EntityHead] ehr
                        INNER JOIN [deb].[Requirement] r ON ehr.[EntityId] = r.[EntityId]
                        INNER JOIN [deb].[StatementRequirement] str ON r.[EntityId] = str.[RequirementId]
                        WHERE str.[StatementId] = st.[EntityId]
                    ) AS RequirementSerialNumbers
                FROM [deb].[Statement] st
                INNER JOIN [common].[EntityHead] eh on st.[EntityId] = eh.[EntityId]
            ");

            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_RequirementSummary]
                AS
                SELECT
                    r.[EntityId],
                    eh.[SerialNumber],
                    eh.[Title],
                    eh.[LastModifiedDate],
                    (
                        SELECT STRING_AGG(s.[Reference], ', ')
                        FROM [deb].[SectionRequirement] sr
                        INNER JOIN [deb].[Section] s ON sr.[SectionID] = s.[Id]
                        WHERE sr.[RequirementID] = r.[EntityId]
                    ) AS SectionReferences
                FROM [deb].[Requirement] r
                INNER JOIN [common].[EntityHead] eh on r.[EntityId] = eh.[EntityId]
            ");

            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_ScopeSummary]
                AS
                SELECT
                  sc.[EntityId],
                  eh.[Title],
                  eh.[OwnedById],
                  eh.[CreatedDate],
                  eh.[LastModifiedDate],
                  COUNT(DISTINCT svr.[StandardVersionId]) AS StandardVersionCount
                FROM [deb].[Scope] sc
                INNER JOIN [common].[EntityHead] eh on sc.[EntityId] = eh.[EntityId]
                LEFT JOIN [deb].[ScopeRequirement] sr ON sc.[EntityId] = sr.[ScopeId]
                LEFT JOIN [deb].[Requirement] r ON sr.[RequirementId] = r.[EntityId]
                LEFT JOIN [deb].[StandardVersionRequirement] svr ON r.[EntityId] = svr.[RequirementId]
                GROUP BY 
                  sc.[EntityId],
                  eh.[Title],
                  eh.[OwnedById],
                  eh.[CreatedDate],
                  eh.[LastModifiedDate]
            ");

            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StandardVersionSummary]
                AS
                SELECT 
					sv.EntityId,
					sv.StandardId,
                    st.[Title] AS StandardTitle,
                    CONCAT(sv.[MajorVersion], ':', sv.[MinorVersion]) AS Version,
                    eh.[Title] AS StandardVersionTitle,
                    sv.[EffectiveStartDate],
                    sv.[EffectiveEndDate],
                    eh.[LastModifiedDate],
					vw.StateID AS StatusId,
					vw.StateTitle AS [Status],
					(SELECT COUNT(DISTINCT sc.EntityId)
					 FROM [deb].[Scope] sc
					 INNER JOIN [deb].[ScopeRequirement] scr ON sc.[EntityId]= scr.ScopeId
					 INNER JOIN [deb].[StandardVersionRequirement] svr ON scr.[RequirementId] = svr.RequirementId AND svr.[StandardVersionId] = sv.[EntityId]) AS ScopeCount
                FROM [deb].[StandardVersion] sv
                INNER JOIN [deb].[Standard] st ON sv.[StandardId] = st.[Id]
                INNER JOIN [common].[EntityHead] eh ON sv.[EntityId] = eh.[EntityId]
				LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StandardVersionSummary]
                AS
                SELECT 
					sv.Id,
					sv.StandardId,
                    st.[Title] AS StandardTitle,
                    CONCAT(sv.[MajorVersion], ':', sv.[MinorVersion]) AS Version,
                    eh.[Title] AS StandardVersionTitle,
                    sv.[EffectiveStartDate],
                    sv.[EffectiveEndDate],
                    eh.[LastModifiedDate],
					vw.StateID AS StatusId,
					vw.StateTitle AS [Status],
					(SELECT COUNT(DISTINCT sc.Id)
					 FROM [deb].[Scope] sc
					 INNER JOIN [deb].[ScopeRequirement] scr ON sc.[Id]= scr.ScopeId
					 INNER JOIN [deb].[StandardVersionRequirement] svr ON scr.[RequirementId] = svr.RequirementId AND svr.[StandardVersionId] = sv.[Id]) AS ScopeCount
                FROM [deb].[StandardVersion] sv
                INNER JOIN [deb].[Standard] st ON sv.[StandardId] = st.[Id]
                INNER JOIN [common].[EntityHead] eh ON sv.[Id] = eh.[EntityId]
				LEFT JOIN [common].[vwPawsState] vw ON eh.[EntityID] = vw.[EntityID]
            ");

            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StatementSummary]
                AS
                SELECT
                    st.[Id],
                    eh.[SerialNumber],
                    eh.[Title],
                    eh.[LastModifiedDate],
					eh.[OwnedById],
                    (
                        SELECT STRING_AGG(ehr.SerialNumber, ', ')
                        FROM [common].[EntityHead] ehr
                        INNER JOIN [deb].[Requirement] r ON ehr.[EntityId] = r.[Id]
                        INNER JOIN [deb].[StatementRequirement] str ON r.[Id] = str.[RequirementId]
                        WHERE str.[StatementId] = st.[Id]
                    ) AS RequirementSerialNumbers
                FROM [deb].[Statement] st
                INNER JOIN [common].[EntityHead] eh on st.[Id] = eh.[EntityId]
            ");

            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_RequirementSummary]
                AS
                SELECT
                    r.[Id],
                    eh.[SerialNumber],
                    eh.[Title],
                    eh.[LastModifiedDate],
                    (
                        SELECT STRING_AGG(s.[Reference], ', ')
                        FROM [deb].[SectionRequirement] sr
                        INNER JOIN [deb].[Section] s ON sr.[SectionID] = s.[Id]
                        WHERE sr.[RequirementID] = r.[Id]
                    ) AS SectionReferences
                FROM [deb].[Requirement] r
                INNER JOIN [common].[EntityHead] eh on r.[Id] = eh.[EntityId]
            ");

            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_ScopeSummary]
                AS
                SELECT
                  sc.[Id],
                  eh.[Title],
                  eh.[OwnedById],
                  eh.[CreatedDate],
                  eh.[LastModifiedDate],
                  COUNT(DISTINCT svr.[StandardVersionId]) AS StandardVersionCount
                FROM [deb].[Scope] sc
                INNER JOIN [common].[EntityHead] eh on sc.[Id] = eh.[EntityId]
                LEFT JOIN [deb].[ScopeRequirement] sr ON sc.[Id] = sr.[ScopeId]
                LEFT JOIN [deb].[Requirement] r ON sr.[RequirementId] = r.[Id]
                LEFT JOIN [deb].[StandardVersionRequirement] svr ON r.[Id] = svr.[RequirementId]
                GROUP BY 
                  sc.[Id],
                  eh.[Title],
                  eh.[OwnedById],
                  eh.[CreatedDate],
                  eh.[LastModifiedDate]
            ");
        }
    }
}
