using System.Collections.ObjectModel;
using System.Diagnostics.SymbolStore;
using System.Reflection;

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

        public static class Dms
        {
            public static class DocumentTypes
            {
                public const string Document = "document";
                public const string Note = "note";
            }

            public static class Libraries
            {
                public const string DebDocuments = "deb-documents";
                public const string CommonDocuments = "common-documents";
                // Add new constants here - they'll automatically be discovered

                /// <summary>
                /// Automatically discovers all public const string fields in this class.
                /// Cached for performance.
                /// </summary>
                private static readonly Lazy<HashSet<string>> _validLibraries = new(() =>
                {
                    var libraryType = typeof(Dms.Libraries);
                    var constants = libraryType
                        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                        .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                        .Select(fi => fi.GetValue(null) as string)
                        .Where(value => !string.IsNullOrWhiteSpace(value))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    return constants!;
                });

                private static HashSet<string> ValidLibraries => _validLibraries.Value;

                public static bool IsValid(string? library)
                {
                    return !string.IsNullOrWhiteSpace(library) && ValidLibraries.Contains(library);
                }

                public static void ValidateOrThrow(string? library)
                {
                    if (string.IsNullOrWhiteSpace(library))
                    {
                        throw new ArgumentException("Library name cannot be null or empty", nameof(library));
                    }

                    if (!ValidLibraries.Contains(library))
                    {
                        throw new ArgumentException($"Invalid library name '{library}'.", nameof(library));
                    }
                }

                public static IReadOnlyCollection<string> GetAll()
                {
                    return ValidLibraries.ToList().AsReadOnly();
                }

                public static bool TryGetNormalized(string? library, out string? normalizedLibrary)
                {
                    normalizedLibrary = null;

                    if (string.IsNullOrWhiteSpace(library))
                    {
                        return false;
                    }

                    normalizedLibrary = ValidLibraries.FirstOrDefault(
                        v => v.Equals(library, StringComparison.OrdinalIgnoreCase));

                    return normalizedLibrary != null;
                }
            }
        }
    }
}