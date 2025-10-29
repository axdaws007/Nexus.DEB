namespace Nexus.DEB.Api.GraphQL
{
    [QueryType]
    public static class HelloQueries
    {
        public static string GetHello() => "Hello from GraphQL!";

        public static string GetGreeting(string name) => $"Hello, {name}!";
    }
}
