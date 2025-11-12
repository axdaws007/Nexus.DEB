namespace Nexus.DEB.Application.Common.Models
{
    public class WorkflowHistoryItem
    {
        public int StepID { get; set; }
        public string? ActivityName { get; set; }
        public string? Status { get; set; }
        public string? SignatureDate { get; set; }
        public string? SignedBy { get; set; }
        public string? SignedByName { get; set; }
        public string? OnBehalfOf { get; set; }
        public string? Comments { get; set; }
        public bool CanUndo { get; set; }
        public bool ShowActivityOwner { get; set; }
        public string? ActivityOwner { get; set; }
    }
}
