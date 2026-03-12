namespace Nexus.DEB.Application.Common.Models.Compliance
{
    public record ComplianceStateResult
    {
        public int? ComplianceStateID { get; init; }
        public string? Label { get; init; }

        public static ComplianceStateResult FromState(int complianceStateId) => new() { ComplianceStateID = complianceStateId };

        public static ComplianceStateResult FromLabel(string label) => new() { Label = label };

        public static ComplianceStateResult Empty => new();
    }
}
