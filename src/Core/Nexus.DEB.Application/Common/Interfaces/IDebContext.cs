namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IDebContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}
