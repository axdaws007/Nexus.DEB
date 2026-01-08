using HotChocolate.Authorization;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class MyWorkQueries
    {
        [Authorize]
        public static async Task<ICollection<MyWorkSummaryItem>> GetMyWorkSummaryItems(
            MyWorkSummaryFilters filters,
            IDebService debService,
            ICurrentUserService currentUserService,
            ICisService cisService,
            ICbacService cbacService,
            CancellationToken cancellationToken)
        {
            List<Guid> roleIds;
            DebHelper.MyWork.FilterTypes.RequiringProgression.Validator.ValidateOrThrow(filters.RequiringProgressionBy);
            DebHelper.MyWork.FilterTypes.CreatedBy.Validator.ValidateOrThrow(filters.CreatedBy);
            DebHelper.MyWork.FilterTypes.OwnedBy.Validator.ValidateOrThrow(filters.OwnedBy);

            var postId = currentUserService.PostId;
            var roles = await cbacService.GetRolesForPostAsync(postId);

            if (roles == null)
                roleIds = [];
            else
                roleIds = [.. roles.Select(x => x.RoleID)];

            var summaryItems = await debService.GetMyWorkSummaryItemsAsync(
                    postId,
                    filters.MyTeamPostIds,
                    filters.ResponsibleGroupIds,
                    filters.CreatedBy,
                    filters.OwnedBy,
                    filters.RequiringProgressionBy,
                    roleIds,
                    cancellationToken);

            return summaryItems ?? [];
        }

        [Authorize]
        public static async Task<ICollection<MyWorkActivity>> GetMyWorkActivities(
            MyWorkActivityFilters filters,
            IDebService debService,
            ICurrentUserService currentUserService,
            ICisService cisService,
            ICbacService cbacService,
            CancellationToken cancellationToken)
        {
            List<Guid> roleIds;
            DebHelper.MyWork.FilterTypes.RequiringProgression.Validator.ValidateOrThrow(filters.RequiringProgressionBy);
            DebHelper.MyWork.FilterTypes.CreatedBy.Validator.ValidateOrThrow(filters.CreatedBy);
            DebHelper.MyWork.FilterTypes.OwnedBy.Validator.ValidateOrThrow(filters.OwnedBy);

            var postId = currentUserService.PostId;
            var roles = await cbacService.GetRolesForPostAsync(postId);

            if (roles == null)
                roleIds = [];
            else
                roleIds = [.. roles.Select(x => x.RoleID)];

            var activities = await debService.GetMyWorkActivitiesAsync(
                    postId,
                    filters.SelectedPostId,
                    filters.EntityTypeTitle,
                    filters.MyTeamPostIds,
                    filters.ResponsibleGroupIds,
                    filters.CreatedBy,
                    filters.OwnedBy,
                    filters.RequiringProgressionBy,
                    roleIds,
                    cancellationToken);

            return activities ?? [];
        }

        [Authorize]
        [UseOffsetPaging]
        [UseSorting]
        public static async Task<ICollection<MyWorkDetailItem>> GetMyWorkDetailItems(
            MyWorkDetailFilters filters,
            IDebService debService,
            ICurrentUserService currentUserService,
            ICisService cisService,
            ICbacService cbacService,
            IApplicationSettingsService applicationSettingsService,
            CancellationToken cancellationToken)
        {
            var moduleId = applicationSettingsService.GetModuleId("DEB");
            var workflowId = await debService.GetWorkflowIdAsync(moduleId, filters.EntityTypeTitle, cancellationToken);

            List <Guid> roleIds;
            DebHelper.MyWork.FilterTypes.RequiringProgression.Validator.ValidateOrThrow(filters.RequiringProgressionBy);
            DebHelper.MyWork.FilterTypes.CreatedBy.Validator.ValidateOrThrow(filters.CreatedBy);
            DebHelper.MyWork.FilterTypes.OwnedBy.Validator.ValidateOrThrow(filters.OwnedBy);

            var postId = currentUserService.PostId;
            var roles = await cbacService.GetRolesForPostAsync(postId);

            if (roles == null)
                roleIds = [];
            else
                roleIds = [.. roles.Select(x => x.RoleID)];

            var details = await debService.GetMyWorkDetailItemsAsync(
                    postId,
                    filters.SelectedPostId,
                    filters.EntityTypeTitle,
                    filters.MyTeamPostIds,
                    filters.ResponsibleGroupIds,
                    filters.CreatedBy,
                    filters.OwnedBy,
                    filters.RequiringProgressionBy,
                    roleIds,
                    filters.ActivityIds,
                    filters.CreatedDateFrom,
                    filters.CreatedDateTo,
                    filters.AssignedDateFrom,
                    filters.AssignedDateTo,
                    workflowId.Value,
                    cancellationToken);

            return details ?? [];
        }
    }
}