namespace Nexus.DEB.Application.Common.Models
{
    public class StatementDetail : EntityDetailBase
    {
        public DateOnly? ReviewDate { get; set; }

        public List<RequirementWithScopes> Requirements { get; set; } = new();
    }
}
