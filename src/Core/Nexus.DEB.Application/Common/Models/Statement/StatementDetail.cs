namespace Nexus.DEB.Application.Common.Models
{
    public class StatementDetail : EntityDetailBase
    {
        public DateTime? ReviewDate { get; set; }

        public List<RequirementWithScopes> Requirements { get; set; } = new();
    }
}
