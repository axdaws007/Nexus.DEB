namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ICorrelationIdAccessor
    {
        string? CorrelationId { get; }
    }
}
