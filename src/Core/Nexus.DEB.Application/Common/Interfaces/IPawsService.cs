namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IPawsService
    {
        Task<IReadOnlyDictionary<Guid, string?>> GetStatusesForEntitiesAsync(
            List<Guid> entityIds,
            CancellationToken cancellationToken = default);
    }
}
