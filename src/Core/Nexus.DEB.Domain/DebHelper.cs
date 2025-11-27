using System.Collections.ObjectModel;

namespace Nexus.DEB.Domain
{
    public static class DebHelper
    {
        public static class ClaimTypes
        {
            public const string Capability = "capability";
            public const string UserName = "username";
            public const string UserId = "userid";
            public const string PostId = "postid";
            public const string PostTitle = "posttitle";
            public const string FirstName = "firstname";
            public const string LastName = "lastname";
        }

        public static class Policies
        {
            public const string CanAddComments = "CanAddComments";
            public const string CanDeleteComments = "CanDeleteComments";
        }

        public static class Capabilites
        {
            public const string CanCreateSoCComments = "CanCreateSoCComments";
            public const string CanCreateScopeComments = "CanCreateScopeComments";
            public const string CanCreateStdVersionComments = "CanCreateStdVersionComments";
            public const string CanCreateRequirementComments = "CanCreateRequirementComments";
            public const string CanCreateTaskComments = "CanCreateTaskComments";
            public const string CanEditSoC = "CanEditSoC";
            public const string CanViewSoCEvidence = "CanViewSoCEvidence";
            public const string CanEditSoCEvidence = "CanEditSoCEvidence";
            public const string CanDeleteSoCEvidence = "CanDeleteSoCEvidence";
            public const string CanAddSocEvidence = "CanAddSocEvidence";
            public const string CanCreateSoCTask = "CanCreateSoCTask";
            public const string CanEditSoCTask = "CanManageSoCTask";
            public const string CanUpVersionStdVersion = "CanUpVersionStdVersion";
            public const string CanEditStdVersion = "CanEditStdVersion";
            public const string CanEditRequirement = "CanEditRequirement";
            public const string CanEditScope = "CanEditScope";
            public const string CanViewReports = "CanViewReports";
            public const string CanViewCommonDocuments = "CanViewCommonDocuments";
            public const string CanEditCommonDocuments = "CanEditCommonDocuments";
            public const string CanDeleteAllSoCComments = "CanDeleteAllSoCComments";
            public const string CanDeleteAllScopeComments = "CanDeleteAllScopeComments";
            public const string CanDeleteAllStdVersionComments = "CanDeleteAllStdVersionComments";
            public const string CanDeleteAllRequirementComments = "CanDeleteAllRequirementComments";
            public const string CanDeleteAllTaskComments = "CanDeleteAllTaskComments";
            public const string CanDeleteOwnedSoCComments = "CanDeleteOwnedSoCComments";
            public const string CanDeleteOwnedScopeComments = "CanDeleteOwnedScopeComments";
            public const string CanDeleteOwnedStdVersionComments = "CanDeleteOwnedStdVersionComments ";
            public const string CanDeleteOwnedRequirementComments = "CanDeleteOwnedRequirementComments";
            public const string CanDeleteOwnedTaskComments = "CanDeleteOwnedTaskComments";

            public static ReadOnlyCollection<string> AllCreateCommentCapabilities => new(
            [
                CanCreateSoCComments,
                CanCreateScopeComments,
                CanCreateStdVersionComments,
                CanCreateRequirementComments,
                CanCreateTaskComments,
            ]);

            public static ReadOnlyCollection<string> AllDeleteAnyCommentCapabilities => new(
            [
                CanDeleteAllSoCComments,
                CanDeleteAllScopeComments,
                CanDeleteAllStdVersionComments,
                CanDeleteAllRequirementComments,
                CanDeleteAllTaskComments,
            ]);

            public static ReadOnlyCollection<string> AllDeleteOwnedCommentCapabilities => new(
            [
                CanDeleteOwnedSoCComments,
                CanDeleteOwnedScopeComments,
                CanDeleteOwnedStdVersionComments,
                CanDeleteOwnedRequirementComments,
                CanDeleteOwnedTaskComments,
            ]);

            public static ReadOnlyCollection<string> AllDeleteCommentCapabilities => AllDeleteAnyCommentCapabilities.Union(AllDeleteOwnedCommentCapabilities).ToList().AsReadOnly();
        }

    }
}