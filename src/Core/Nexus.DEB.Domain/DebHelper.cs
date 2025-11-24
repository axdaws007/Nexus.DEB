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
            public const string CanEditSoC = "CanEditSoC";
            public const string CanViewSoCEvidence = "CanViewSoCEvidence";
            public const string CanEditSoCEvidence = "CanManageSoCEvidence";
            public const string CanDeleteSoCEvidence = "CanDeleteSoCEvidence";
            public const string CanAddSocEvidence = "CanAddSocEvidence";
            public const string CanEditSoCTask = "CanManageSoCTask";
            public const string CanUpVersionSoC = "CanUpVersionSoC";
            public const string CanEditStdVersion = "CanManageStdVersion";
            public const string CanEditRequirement = "CanManageRequirement";
            public const string CanEditScope = "CanManageScope";
            public const string CanEditSoCComments = "CanEditSoCComments ";
            public const string CanEditScopeComments = "CanEditScopeComments";
            public const string CanEditStdVersionComments = "CanEditStdVersionComments";
            public const string CanEditRequirementComments = "CanEditRequirementComments ";
            public const string CanEditTaskComments = "CanEditTaskComments ";
            public const string CanViewReports = "CanViewReports";
            public const string CanViewCommonDocuments = "CanViewCommonDocuments";
            public const string CanEditCommonDocuments = "CanEditCommonDocuments";

            public static ReadOnlyCollection<string> AllEditCommentCapabilities => new(
            [
                CanEditSoCComments,
                CanEditScopeComments,
                CanEditStdVersionComments,
                CanEditRequirementComments,
                CanEditTaskComments,
            ]);
        }

    }
}