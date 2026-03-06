using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.DEB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendedPropertiesForRequirement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"

EXEC sys.sp_addextendedproperty 
	@name = N'MS_Description',
	@value = N'Effective Start Date',
	@level0type = N'SCHEMA', @level0name = N'deb',
	@level1type = N'TABLE',  @level1name = N'Requirement',
	@level2type = N'COLUMN', @level2name = N'EffectiveStartDate';

EXEC sys.sp_addextendedproperty 
	@name = N'MS_Description',
	@value = N'EffectiveEndDate',
	@level0type = N'SCHEMA', @level0name = N'deb',
	@level1type = N'TABLE',  @level1name = N'Requirement',
	@level2type = N'COLUMN', @level2name = N'EffectiveEndDate';

EXEC sys.sp_addextendedproperty 
	@name = N'MS_Description',
	@value = N'Is Title Displayed?',
	@level0type = N'SCHEMA', @level0name = N'deb',
	@level1type = N'TABLE',  @level1name = N'Requirement',
	@level2type = N'COLUMN', @level2name = N'IsTitleDisplayed';

EXEC sys.sp_addextendedproperty 
	@name = N'MS_Description',
	@value = N'Is Reference Displayed?',
	@level0type = N'SCHEMA', @level0name = N'deb',
	@level1type = N'TABLE',  @level1name = N'Requirement',
	@level2type = N'COLUMN', @level2name = N'IsReferenceDisplayed';

EXEC sys.sp_addextendedproperty 
	@name = N'MS_Description',
	@value = N'Requirement Category',
	@level0type = N'SCHEMA', @level0name = N'deb',
	@level1type = N'TABLE',  @level1name = N'Requirement',
	@level2type = N'COLUMN', @level2name = N'RequirementCategoryId';

EXEC sys.sp_addextendedproperty 
	@name = N'MS_Description',
	@value = N'Requirement Type',
	@level0type = N'SCHEMA', @level0name = N'deb',
	@level1type = N'TABLE',  @level1name = N'Requirement',
	@level2type = N'COLUMN', @level2name = N'RequirementTypeId';

EXEC sys.sp_addextendedproperty 
	@name = N'MS_Description',
	@value = N'Compliance Weighting',
	@level0type = N'SCHEMA', @level0name = N'deb',
	@level1type = N'TABLE',  @level1name = N'Requirement',
	@level2type = N'COLUMN', @level2name = N'ComplianceWeighting';
");
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"
EXEC sys.sp_dropextendedproperty 
				@name = N'MS_Description',
				@level0type = N'SCHEMA', @level0name = N'deb',
				@level1type = N'TABLE',  @level1name = N'Requirement',
				@level2type = N'COLUMN', @level2name = N'EffectiveStartDate';

EXEC sys.sp_dropextendedproperty 
				@name = N'MS_Description',
				@level0type = N'SCHEMA', @level0name = N'deb',
				@level1type = N'TABLE',  @level1name = N'Requirement',
				@level2type = N'COLUMN', @level2name = N'EffectiveEndDate';

EXEC sys.sp_dropextendedproperty 
				@name = N'MS_Description',
				@level0type = N'SCHEMA', @level0name = N'deb',
				@level1type = N'TABLE',  @level1name = N'Requirement',
				@level2type = N'COLUMN', @level2name = N'IsTitleDisplayed';

EXEC sys.sp_dropextendedproperty 
				@name = N'MS_Description',
				@level0type = N'SCHEMA', @level0name = N'deb',
				@level1type = N'TABLE',  @level1name = N'Requirement',
				@level2type = N'COLUMN', @level2name = N'IsReferenceDisplayed';

EXEC sys.sp_dropextendedproperty 
				@name = N'MS_Description',
				@level0type = N'SCHEMA', @level0name = N'deb',
				@level1type = N'TABLE',  @level1name = N'Requirement',
				@level2type = N'COLUMN', @level2name = N'RequirementCategoryId';

EXEC sys.sp_dropextendedproperty 
				@name = N'MS_Description',
				@level0type = N'SCHEMA', @level0name = N'deb',
				@level1type = N'TABLE',  @level1name = N'Requirement',
				@level2type = N'COLUMN', @level2name = N'RequirementTypeId';

EXEC sys.sp_dropextendedproperty 
				@name = N'MS_Description',
				@level0type = N'SCHEMA', @level0name = N'deb',
				@level1type = N'TABLE',  @level1name = N'Requirement',
				@level2type = N'COLUMN', @level2name = N'ComplianceWeighting';
");
		}
    }
}
