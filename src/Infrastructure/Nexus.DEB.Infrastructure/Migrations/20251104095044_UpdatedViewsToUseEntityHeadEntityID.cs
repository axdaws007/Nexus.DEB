using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedViewsToUseEntityHeadEntityID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StandardVersionSummary]
                AS
                SELECT 
                    -- StandardVersion Id
					sv.Id,
                    -- Standard Title
                    st.[Title] AS StandardTitle,
                    
                    -- Version (concatenated MajorVersion:MinorVersion)
                    CONCAT(sv.[MajorVersion], ':', sv.[MinorVersion]) AS Version,
                    
                    -- StandardVersion Title from EntityHead
                    eh.[Title] AS StandardVersionTitle,
                    
                    -- Effective date range
                    sv.[EffectiveStartDate],
                    sv.[EffectiveEndDate],
                    
                    -- Last modified information from EntityHead
                    eh.[LastModifiedDate],
                    
                    -- Count of distinct Scopes related to this StandardVersion
                    COUNT(DISTINCT sc.[Id]) AS ScopeCount

                FROM 
                    [deb].[StandardVersion] sv
                    
                    -- Join to get Standard title
                    INNER JOIN [deb].[Standard] st ON sv.[StandardId] = st.[Id]
                    
                    -- Join to get EntityHead information (Title, LastModifiedDate)
                    INNER JOIN [common].[EntityHead] eh ON sv.[Id] = eh.[EntityId]
                    
                    -- Left joins to count related Scopes through the requirement relationships
                    LEFT JOIN [deb].[StandardVersionRequirement] svr ON sv.[Id] = svr.[StandardVersionId]
                    
                    LEFT JOIN [deb].[ScopeRequirement] sr ON svr.[RequirementId] = sr.[RequirementId]
                    
                    LEFT JOIN [deb].[Scope] sc ON sr.[ScopeId] = sc.[Id]

                GROUP BY 
                    st.[Title],
                    sv.[MajorVersion],
                    sv.[MinorVersion],
                    eh.[Title],
                    sv.[EffectiveStartDate],
                    sv.[EffectiveEndDate],
                    eh.[LastModifiedDate],
                    sv.[Id];
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER VIEW [deb].[vw_StandardVersionSummary]
                AS
                SELECT 
                    -- StandardVersion Id
					sv.Id,
                    -- Standard Title
                    st.[Title] AS StandardTitle,
                    
                    -- Version (concatenated MajorVersion:MinorVersion)
                    CONCAT(sv.[MajorVersion], ':', sv.[MinorVersion]) AS Version,
                    
                    -- StandardVersion Title from EntityHead
                    eh.[Title] AS StandardVersionTitle,
                    
                    -- Effective date range
                    sv.[EffectiveStartDate],
                    sv.[EffectiveEndDate],
                    
                    -- Last modified information from EntityHead
                    eh.[LastModifiedDate],
                    
                    -- Count of distinct Scopes related to this StandardVersion
                    COUNT(DISTINCT sc.[Id]) AS ScopeCount

                FROM 
                    [deb].[StandardVersion] sv
                    
                    -- Join to get Standard title
                    INNER JOIN [deb].[Standard] st 
                        ON sv.[StandardId] = st.[Id]
                    
                    -- Join to get EntityHead information (Title, LastModifiedDate)
                    INNER JOIN [common].[EntityHead] eh 
                        ON sv.[Id] = eh.[Id]
                    
                    -- Left joins to count related Scopes through the requirement relationships
                    LEFT JOIN [deb].[StandardVersionRequirement] svr 
                        ON sv.[Id] = svr.[StandardVersionId]
                    
                    LEFT JOIN [deb].[ScopeRequirement] sr 
                        ON svr.[RequirementId] = sr.[RequirementId]
                    
                    LEFT JOIN [deb].[Scope] sc 
                        ON sr.[ScopeId] = sc.[Id]

                GROUP BY 
                    st.[Title],
                    sv.[MajorVersion],
                    sv.[MinorVersion],
                    eh.[Title],
                    sv.[EffectiveStartDate],
                    sv.[EffectiveEndDate],
                    eh.[LastModifiedDate],
                    sv.[Id];
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
                INNER JOIN [common].[EntityHead] eh on sc.[Id] = eh.[Id]
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
                INNER JOIN [common].[EntityHead] eh on r.[Id] = eh.[Id]
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
                        INNER JOIN [deb].[Requirement] r ON ehr.[Id] = r.[Id]
                        INNER JOIN [deb].[StatementRequirement] str ON r.[Id] = str.[RequirementId]
                        WHERE str.[StatementId] = st.[Id]
                    ) AS RequirementSerialNumbers
                FROM [deb].[Statement] st
                INNER JOIN [common].[EntityHead] eh on st.[Id] = eh.[Id]
            ");
        }
    }
}
