using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDataToComplianceStateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"SET IDENTITY_INSERT [compliance].[ComplianceState] ON ");
            migrationBuilder.Sql(@"INSERT [compliance].[ComplianceState] ([ComplianceStateID], [Name], [Description], [DisplayOrder], [Colour], [IsTerminal], [IsActive]) VALUES (1, N'Not Started', N'Not Started', 1, N'#9E9E9E', 0, 1)");
            migrationBuilder.Sql(@"INSERT [compliance].[ComplianceState] ([ComplianceStateID], [Name], [Description], [DisplayOrder], [Colour], [IsTerminal], [IsActive]) VALUES (2, N'In Progress', N'In Progress', 2, N'#FFA500', 0, 1)");
            migrationBuilder.Sql(@"INSERT [compliance].[ComplianceState] ([ComplianceStateID], [Name], [Description], [DisplayOrder], [Colour], [IsTerminal], [IsActive]) VALUES (3, N'In Review', N'In Review', 3, N'#2196F3', 0, 1)");
            migrationBuilder.Sql(@"INSERT [compliance].[ComplianceState] ([ComplianceStateID], [Name], [Description], [DisplayOrder], [Colour], [IsTerminal], [IsActive]) VALUES (4, N'Complete', N'Complete', 4, N'#4CAF50', 1, 1)");
            migrationBuilder.Sql(@"INSERT [compliance].[ComplianceState] ([ComplianceStateID], [Name], [Description], [DisplayOrder], [Colour], [IsTerminal], [IsActive]) VALUES (5, N'Attention', N'Attention', 5, N'#FF0000', 0, 1)");
            migrationBuilder.Sql(@"SET IDENTITY_INSERT [compliance].[ComplianceState] OFF");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM [compliance].[ComplianceState]");
        }
    }
}
