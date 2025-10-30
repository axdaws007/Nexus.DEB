namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        Guid PostId { get; }
        string? UserName { get; }
        bool IsAuthenticated { get; }
    }
}
