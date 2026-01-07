using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedPropertiesToTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"

EXEC sys.sp_addextendedproperty 
	@name = N'MS_Description',
	@value = N'Task Type',
	@level0type = N'SCHEMA', @level0name = N'deb',
	@level1type = N'TABLE',  @level1name = N'Task',
	@level2type = N'COLUMN', @level2name = N'TaskTypeId';

EXEC sys.sp_addextendedproperty 
	@name = N'MS_Description',
	@value = N'Statement',
	@level0type = N'SCHEMA', @level0name = N'deb',
	@level1type = N'TABLE',  @level1name = N'Task',
	@level2type = N'COLUMN', @level2name = N'StatementId';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"
EXEC sys.sp_dropextendedproperty 
				@name = N'MS_Description',
				@level0type = N'SCHEMA', @level0name = N'deb',
				@level1type = N'TABLE',  @level1name = N'Task',
				@level2type = N'COLUMN', @level2name = N'TaskTypeId';

EXEC sys.sp_dropextendedproperty 
				@name = N'MS_Description',
				@level0type = N'SCHEMA', @level0name = N'deb',
				@level1type = N'TABLE',  @level1name = N'Task',
				@level2type = N'COLUMN', @level2name = N'StatementId';
");
		}
    }
}
