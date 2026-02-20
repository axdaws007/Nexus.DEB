using Nexus.DEB.Domain.Models.Common;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using static Nexus.DEB.Domain.DebHelper.Dms;

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

        public static class Paws
        {
            public static class States
            {
                public const string Draft = "Draft";
                public const string Open = "Open";
                public const string Closed = "Closed";
                public const string Active = "Active";
                public const string ClosedArchived = "Closed/Archived";
                public const string Superceded = "Superceded";
                public const string Archived = "Archived";
                public const string Auditable = "Auditable";
                public const string Audited = "Audited";
                public const string ClosedAndArchived = "Closed and Archived";


                public static ReadOnlyCollection<string> AllEditableStates => new(
                [
                    Draft
                ]);
            }

            public static class Status
            {
                public const int Pending = 1;
            }

            public static class MutTags
            {
                public const string DashboardOpened = "DashboardOpened";
            }
        }

        public static class Policies
        {
            public const string CanAddComments = "CanAddComments";
            public const string CanDeleteComments = "CanDeleteComments";
            public const string CanEditStdVersion = "CanEditStdVersion";
            public const string CanCreateOrEditRequirement = "CanCreateOrEditRequirement";
            public const string CanCreateOrEditSoC = "CanCreateOrEditSoC";
			public const string CanCreateOrEditScope = "CanCreateOrEditScope";
			public const string CanCreateSoCTask = "CanCreateSoCTask";
            public const string CanEditSoCTask = "CanEditSoCTask";
            public const string CanAddDocuments = "CanAddDocuments";
            public const string CanDeleteDocuments = "CanDeleteDocuments";
            public const string CanEditDocuments = "CanEditDocuments";
			public const string CanViewDocuments = "CanViewDocuments";
		}

        public static class Capabilites
        {
			#region SoC
			public const string CanEditSoC = "CanEditSoC";
            public const string CanViewSoCEvidence = "CanViewSoCEvidence";
            public const string CanEditSoCEvidence = "CanEditSoCEvidence";
            public const string CanDeleteSoCEvidence = "CanDeleteSoCEvidence";
            public const string CanAddSoCEvidence = "CanAddSocEvidence";
            public const string CanCreateSoCTask = "CanCreateSoCTask";
            public const string CanEditSoCTask = "CanEditSoCTask";
			#endregion SoC

			#region StandardVersion
			public const string CanUpVersionStdVersion = "CanUpVersionStdVersion";
            public const string CanEditStdVersion = "CanEditStdVersion";
			#endregion StandardVersion

			#region Requirement
			public const string CanEditRequirement = "CanEditRequirement";
			#endregion Requirement

			#region Scope
			public const string CanEditScope = "CanEditScope";
            public const string CanViewScopeAttachments = "CanViewScopeAttachments";
			public const string CanCreateScopeAttachments = "CanCreateScopeAttachments";
			public const string CanEditScopeAttachments = "CanEditScopeAttachments";
			public const string CanDeleteScopeAttachments = "CanDeleteScopeAttachments";
			#endregion Scope

			public const string CanViewReports = "CanViewReports";
            public const string CanViewCommonDocuments = "CanViewCommonDocuments";
            public const string CanEditCommonDocuments = "CanEditCommonDocuments";
			
            #region Comments
			public const string CanCreateSoCComments = "CanCreateSoCComments";
			public const string CanCreateScopeComments = "CanCreateScopeComments";
			public const string CanCreateStdVersionComments = "CanCreateStdVersionComments";
			public const string CanCreateRequirementComments = "CanCreateRequirementComments";
			public const string CanCreateTaskComments = "CanCreateTaskComments";
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
            #endregion Comments


            public static readonly IReadOnlyDictionary<string, string> EditCapabilityByEntityType = new Dictionary<string, string>
            {
                [EntityTypes.StandardVersion] = DebHelper.Capabilites.CanEditStdVersion,
                [EntityTypes.Requirement] = DebHelper.Capabilites.CanEditRequirement,
                [EntityTypes.SoC] = DebHelper.Capabilites.CanEditSoC,
                [EntityTypes.Task] = DebHelper.Capabilites.CanEditSoCTask,
                [EntityTypes.Scope] = DebHelper.Capabilites.CanEditScope
            };

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

            public static ReadOnlyCollection<string> AllCreateDocCapabilities => new(
			[
				CanAddSoCEvidence,
				CanCreateScopeAttachments,
			]);

			public static ReadOnlyCollection<string> AllDeleteDocCapabilities => new(
			[
				CanDeleteSoCEvidence,
				CanDeleteScopeAttachments,
			]);

			public static ReadOnlyCollection<string> AllEditDocCapabilities => new(
			[
				CanEditCommonDocuments,
				CanEditSoCEvidence,
				CanEditScopeAttachments,
			]);

			public static ReadOnlyCollection<string> AllViewDocCapabilities => new(
			[
				CanViewCommonDocuments,
				CanViewSoCEvidence,
				CanViewScopeAttachments,
			]);

			public static ReadOnlyCollection<string> AllDeleteCommentCapabilities => AllDeleteAnyCommentCapabilities.Union(AllDeleteOwnedCommentCapabilities).ToList().AsReadOnly();
        }

        public static class MyWork
        {
            private const string ANYONE = "ANYONE";
            private const string MYPOST = "MYPOST";
            private const string MYTEAM = "MYTEAM";
            private const string MYROLES = "MYROLES";
            private const string GROUP = "GROUP";

            public static class FilterTypes
            {
                public static class RequiringProgression
                {
                    public const string Anyone = ANYONE;
                    public const string MyPost = MYPOST;
                    public const string MyTeam = MYTEAM;
                    public const string MyRoles = MYROLES;

                    public static ConstantStringValidator Validator { get; } = new(typeof(RequiringProgression));
                }

                public static class CreatedBy
                {
                    public const string Anyone = ANYONE;
                    public const string MyPost = MYPOST;
                    public const string MyTeam = MYTEAM;
                    public static ConstantStringValidator Validator { get; } = new(typeof(CreatedBy));
                }

                public static class OwnedBy
                {
                    public const string Anyone = ANYONE;
                    public const string MyPost = MYPOST;
                    public const string MyTeam = MYTEAM;
                    public const string Group = GROUP;
                    public static ConstantStringValidator Validator { get; } = new(typeof(OwnedBy));
                }

                public static ImmutableDictionary<string, int> MapToIntegerValue = new Dictionary<string, int>()
                {
                    { ANYONE, 0 },
                    { MYPOST, 1 },
                    { MYTEAM, 2 },
                    { MYROLES, 3 },
                    { GROUP, 4 }
                }.ToImmutableDictionary();
            }
        }

        public static class Dms
        {
            public static class DocumentTypes
            {
                public const string Document = "document";
                public const string Note = "note";

                public static ConstantStringValidator Validator { get; } = new(typeof(DocumentTypes));
            }

            public static class Libraries
            {
                public const string DebDocuments = "deb-documents";
                public const string CommonDocuments = "common-documents";
                // Add new constants here - they'll automatically be discovered

                public static ConstantStringValidator Validator { get; } = new(typeof(Libraries));
            }
        }

    }
}

public sealed class ConstantStringValidator
{
    private readonly Lazy<HashSet<string>> _validValues;
    private readonly string _valueName;

    public ConstantStringValidator(Type constantsType, string? overrrideValueName = null)
    {
        if (!string.IsNullOrEmpty(overrrideValueName))
        {
            _valueName = overrrideValueName;
        }
        else
        {
            _valueName = constantsType.Name;
        }

        _validValues = new Lazy<HashSet<string>>(() =>
        {
            var constants = constantsType
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(fi => fi.GetValue(null) as string)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)!;

            return constants;
        });
    }

    private HashSet<string> ValidValues => _validValues.Value;

    public bool IsValid(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && ValidValues.Contains(value);
    }

    public void ValidateOrThrow(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{_valueName} cannot be null or empty", nameof(value));
        }

        if (!ValidValues.Contains(value))
        {
            throw new ArgumentException($"Invalid {_valueName} '{value}'.", nameof(value));
        }
    }

    public IReadOnlyCollection<string> GetAll()
    {
        return ValidValues.ToList().AsReadOnly();
    }

    public bool TryGetNormalized(string? value, out string? normalizedValue)
    {
        normalizedValue = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        normalizedValue = ValidValues.FirstOrDefault(
            v => v.Equals(value, StringComparison.OrdinalIgnoreCase));

        return normalizedValue != null;
    }
}
