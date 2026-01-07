namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IDebContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Guid EventId { get; }
        string? UserDetails { get; }
        Task SetFormattedUser();
	}
}
