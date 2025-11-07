namespace Nexus.DEB.Api.Restful.Models
{
    public class StatementAndTasksParameters
    {
        public Guid StandardVersionId { get; set; }
        public List<Guid>? PossiblePostIds { get; set; }
        public short MaximumNumberOfTasksPerStatement { get; set; } = 3;
    }
}
