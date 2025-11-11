using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Api.GraphQL
{
    public class TransitionValidationResult
    {
        public bool CanProceed { get; set; }
        public List<ValidationError> ValidationErrors { get; set; } = new();
    }
}
