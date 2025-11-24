namespace Nexus.DEB.Application.Common.Models
{
    public class StatementDetail : EntityDetailBase
    {
        public string StatementText { get; set; } = string.Empty;
        public DateTime? ReviewDate { get; set; }

        public List<RequirementWithScopes> Requirements { get; set; } = new();
    }
}
