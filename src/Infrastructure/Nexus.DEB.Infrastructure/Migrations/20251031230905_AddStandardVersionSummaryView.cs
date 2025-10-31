using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStandardVersionSummaryView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW [deb].[vw_StandardVersionSummary]
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP VIEW IF EXISTS [deb].[vw_StandardVersionSummary];
            ");
        }
    }
}
