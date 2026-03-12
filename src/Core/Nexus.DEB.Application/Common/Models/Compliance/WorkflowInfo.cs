namespace Nexus.DEB.Application.Common.Models.Compliance
{
    public record WorkflowInfo(Guid WorkflowId, int ActivityId, int StatusId, int? PseudoStateId);
}
