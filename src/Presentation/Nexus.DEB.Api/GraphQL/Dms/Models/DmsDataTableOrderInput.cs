namespace Nexus.DEB.Api.GraphQL
{
    public class DmsDataTableOrderInput
    {
        public string? ColumnName { get; set; }

        public string? Dir { get; set; } // "asc" or "desc"
    }
}
