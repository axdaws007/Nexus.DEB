namespace Nexus.DEB.Infrastructure.Helpers
{
    public static class StringHelper
    {
        public static string? Truncate(string? value, int maxLength)
            => value?.Length > maxLength ? value[..maxLength] : value;
    }
}
