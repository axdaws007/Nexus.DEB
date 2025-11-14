using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class StepApprovalResult
    {

        public bool IsApproved { get; set; }
        public bool AreSideEffectsSuccessful { get; set; }
        public bool OverallSuccess => IsApproved && AreSideEffectsSuccessful;

        public List<ValidationError> Errors { get; set; } = new();
    }
}
