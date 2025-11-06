namespace Nexus.DEB.Application.Common.Models.Filters
{
    public class FilterItemEntity
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class FilterItem
    {
        public short Id { get; set; }
        public string Value { get; set; }
        public bool IsEnabled { get; set; }
    }
}
