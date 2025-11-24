namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IDebUser
    {
        Guid UserId { get; }
        Guid PostId { get; }
        string FirstName { get; }
        string LastName { get; }
        string UserName { get; }
        string PostTitle { get; }
        string FirstNameInitialAndLastName { get; }
        ICollection<string> Capabilities { get; }
    }
}
