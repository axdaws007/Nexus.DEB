using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Domain;
using System.Security.Claims;

namespace Nexus.DEB.Api.Security
{
    public class DebUser : IDebUser
    {
        public bool IsAuthenticated { get; init; } = false;

        public Guid UserId { get; init; } = Guid.Empty;
        public Guid PostId { get; init; } = Guid.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string PostTitle { get; init; } = string.Empty;
        public ICollection<string> Capabilities { get; init; } = [];

        public string FirstNameInitialAndLastName => $"{(FirstName.Length > 0 ? FirstName[0] : string.Empty)} {LastName}".Trim();

        public DebUser(ClaimsPrincipal? claimsPrincipal)
        {
            if (claimsPrincipal?.Identity?.IsAuthenticated == true)
            {
                IsAuthenticated = true;

                UserId = (from claim in claimsPrincipal.Claims
                          where claim.Type == DebHelper.ClaimTypes.UserId
                          select Guid.Parse(claim.Value)).First();

                PostId = (from claim in claimsPrincipal.Claims
                          where claim.Type == DebHelper.ClaimTypes.PostId
                          select Guid.Parse(claim.Value)).First();

                UserName = (from claim in claimsPrincipal.Claims
                            where claim.Type == DebHelper.ClaimTypes.UserName
                            select claim.Value).First();

                FirstName = (from claim in claimsPrincipal.Claims
                             where claim.Type == DebHelper.ClaimTypes.FirstName
                             select claim.Value).First();

                LastName = (from claim in claimsPrincipal.Claims
                            where claim.Type == DebHelper.ClaimTypes.LastName
                            select claim.Value).First();

                PostTitle = (from claim in claimsPrincipal.Claims
                            where claim.Type == DebHelper.ClaimTypes.PostTitle
                            select claim.Value).First();

                Capabilities = (from claim in claimsPrincipal.Claims
                                where claim.Type == DebHelper.ClaimTypes.Capability
                                select claim.Value).ToList();
            }
        }
    }
}
