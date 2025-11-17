using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class TransitionDetail
    {
        public bool ValidationSuccessful { get; set; }
        public bool ShowSignOffText { get; set; }
        public string? SignOffText { get; set; }
        public bool RequirePassword { get; set; }

        public List<ValidationError> ValidationErrors { get; set; } = new();
        public ICollection<TargetActivity>? TargetActivities { get; set; }

    }
}
