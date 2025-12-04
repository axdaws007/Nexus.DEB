namespace Nexus.DEB.Api.GraphQL
{
    public class DmsDataTableParametersInput
    {
        public int Draw { get; set; }

        public int Start { get; set; }

        public int Length { get; set; } = 10;

        public List<DmsDataTableOrderInput> Order { get; set; } = new();
    }
}
